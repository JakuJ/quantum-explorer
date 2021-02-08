using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.CompilationBuilder;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.Simulation.Core;
using Simulator;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;
using Diagnostic = Microsoft.VisualStudio.LanguageServer.Protocol.Diagnostic;
using DiagnosticSeverity = Microsoft.VisualStudio.LanguageServer.Protocol.DiagnosticSeverity;

namespace Compiler
{
    /// <inheritdoc cref="ICompiler"/>
    public class QsCompiler : ICompiler
    {
        private static string[]? qsharpReferences;
        private static string[]? csharpReferences;
        private static References? cachedRefs;

        private readonly ILogger logger;
        private readonly string filename = $"__{UniqueId.CreateUniqueId()}__.qs";

        private static void InitializeReferences()
        {
            // necessary references to compile our Q# program
            qsharpReferences ??= new[]
                {
                    "Microsoft.Quantum.Standard",
                    "Microsoft.Quantum.QSharp.Core",
                    "Microsoft.Quantum.Runtime.Core",
                    typeof(InterceptingSimulator).Assembly.FullName!,
                }.Select(x => Assembly.Load(new AssemblyName(x)).Location)
                 .ToArray();

            // necessary references to compile C# simulation of the Q# compilation
            csharpReferences ??= new[]
                {
                    "Microsoft.Quantum.Simulators",
                    "Microsoft.Quantum.EntryPointDriver",
                    "System.CommandLine",
                    "System.Runtime",
                    "netstandard",
                    "System.Collections.Immutable",
                    typeof(object).Assembly.FullName!, // never null
                }.Select(x => Assembly.Load(new AssemblyName(x)).Location)
                 .Concat(qsharpReferences)
                 .ToArray();
        }

        /// <summary>Initializes a new instance of the <see cref="QsCompiler"/> class.</summary>
        /// <param name="logger">An <see cref="ILogger"/> instance to log compilation messages with.</param>
        public QsCompiler(ILogger logger)
        {
            this.logger = logger;
            InitializeReferences();
        }

        /// <inheritdoc/>
        public event EventHandler<string>? OnDiagnostics;

        /// <inheritdoc/>
        public event EventHandler<Dictionary<string, List<GateGrid>>>? OnGrids;

        /// <inheritdoc/>
        public event EventHandler<string>? OnOutput;

        /// <inheritdoc/>
        public event EventHandler<List<OperationState>>? OnStatesRecorded;

        /// <inheritdoc/>
        public async Task Compile(string qsharpCode, bool expanding = false)
        {
            // Do we have an uncommented @EntryPoint?
            if (!Regex.IsMatch(qsharpCode, @"(?<!//.*)@EntryPoint"))
            {
                OnDiagnostics?.Invoke(this, "Nothing to execute, no entry point specified.");
                return;
            }

            // Auto-open Simulator.Custom in all namespaces
            List<string> userNamespaces = new();

            qsharpCode = Regex.Replace(qsharpCode, @"namespace\s+([\w\.]+)\s*{", match =>
            {
                userNamespaces.Add(match.Groups.Values.ElementAt(1).Captures.Single().Value);
                return match.Value + "open Simulator.Custom;";
            });

            // to load our custom rewrite step, we need to point Q# compiler config at the rewrite step
            InMemoryEmitter emitter = new();
            var config = new CompilationLoader.Configuration
            {
                IsExecutable = true,
                SkipMonomorphization = true, // performs calls to PrependGuid causing some library methods not to be recognized
                GenerateFunctorSupport = true,
                RewriteStepInstances = new (IRewriteStep, string?)[]
                {
                    (new AllocationTagger(userNamespaces), null),
                    (emitter, null),
                },
            };

            // compile Q# code
            CompilationLoader? compilationLoader;

            using (new ScopedTimer("Compiling Q# code", logger))
            {
                try
                {
                    compilationLoader = new CompilationLoader(
                        _ => new Dictionary<Uri, string> { { new Uri(Path.GetFullPath(filename)), qsharpCode } }.ToImmutableDictionary(),
                        load => cachedRefs ??= load(qsharpReferences!), // never null
                        config,
                        new EventLogger(str => OnDiagnostics?.Invoke(this, str)));
                }
                catch (NullReferenceException)
                {
                    // Bond DLL deserialization throws this from QDK v0.13.* onwards when IsExecutable is true but the user provides no @EntryPoint
                    // We have unit tests assuring that this should never happen
                    // This try/catch block exists just to be extra safe
                    logger.LogError(
                        $"{nameof(NullReferenceException)} raised during Q# compilation. Presumably missing @EntryPoint in code:\n{qsharpCode}");
                    OnDiagnostics?.Invoke(this, "No entry point operation specified.\nDecorate the main method with the @EntryPoint attribute.");
                    return;
                }
            }

            // print any diagnostics
            ImmutableArray<Diagnostic> diags = compilationLoader.LoadDiagnostics;
            if (diags.Any())
            {
                var diagnostics = string.Join(Environment.NewLine, diags.Select(d => $"{d.Severity} {d.Code} {d.Message}"));

                OnDiagnostics?.Invoke(this, diagnostics);

                // if there are any errors, exit
                if (diags.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    return;
                }
            }

            CSharpCompilation? csharpCompilation;
            using (new ScopedTimer("Compiling C# driver code", logger))
            {
                // we captured the emitted C# syntax trees into a static variable in the rewrite step
                IEnumerable<SyntaxTree> syntaxTrees = emitter.FileContents.Select(x => CSharpSyntaxTree.ParseText(x.Value));

                // compile C# code
                // make sure to pass in the C# references as Roslyn's metadata references
                csharpCompilation = CSharpCompilation.Create("qsharp-assembly", syntaxTrees)
                                                     .WithReferences(csharpReferences!.Select(x => MetadataReference.CreateFromFile(x)));
            }

            // print any diagnostics
            var csharpDiagnostics = csharpCompilation
                                   .GetDiagnostics()
                                   .Where(d => d is { Severity: not Hidden, Id: not "CS1701" and not "CS1702" })
                                   .ToList();
            if (csharpDiagnostics.Any())
            {
                var diagnostics = string.Join("", csharpDiagnostics.Select(d => $"\n{d.Severity} {d.Id} {d.GetMessage()}"));

                OnDiagnostics?.Invoke(this, diagnostics);

                // if there are any errors, exit
                if (csharpDiagnostics.Any(d => d.Severity == Error))
                {
                    return;
                }
            }

            // measure execution time
            using var timer = new ScopedTimer("Executing simulation", logger);

            // emit C# code into an in-memory assembly
            await using MemoryStream peStream = new();
            csharpCompilation.Emit(peStream);
            peStream.Position = 0;

            // load that assembly
            QSharpLoadContext qsharpLoadContext = new();
            Assembly qsharpAssembly = qsharpLoadContext.LoadFromStream(peStream);

            // get the @EntryPoint() operation
            QsQualifiedName? entryPoint = compilationLoader.CompilationOutput?.EntryPoints.FirstOrDefault();

            if (entryPoint == null)
            {
                logger.LogError("No entrypoint in Q# compilation.");
                qsharpLoadContext.Unload();
                return;
            }

            Type? type = qsharpAssembly.GetExportedTypes().FirstOrDefault(x => x.FullName == $"{entryPoint.Namespace}.{entryPoint.Name}");

            if (type != null)
            {
                using InterceptingSimulator sim = new(userNamespaces, expanding, logger);
                StateRecorder recorder = new(sim);

                var simSuccess = true;
                try
                {
                    // simulate the entry point operation using reflection
                    object? invocation = type.InvokeMember("Run", BindingFlags.InvokeMethod, null, type, new object?[] { sim });

                    if (invocation is Task task)
                    {
                        await task;
                    }
                }
                catch (ExecutionFailException e)
                {
                    OnDiagnostics?.Invoke(this, e.Message);
                    simSuccess = false;
                }

                if (simSuccess)
                {
                    OnOutput?.Invoke(this, sim.Messages);
                    OnStatesRecorded?.Invoke(this, recorder.Root.Children);

                    Dictionary<string, List<GateGrid>> grids = sim.GetGrids();

                    if (grids.Count > 0)
                    {
                        OnGrids?.Invoke(this, grids);
                    }
                }
            }
            else
            {
                logger.LogError("Q# entrypoint not found in loaded assembly.");
            }

            qsharpLoadContext.Unload();
        }
    }
}
