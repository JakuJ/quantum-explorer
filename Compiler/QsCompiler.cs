using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.SyntaxTree;

namespace Compiler
{
    /// <inheritdoc />
    /// <remarks>
    /// As per <see cref="!:https://www.strathweb.com/2020/08/running-q-compiler-and-simulation-programmatically-from-a-c-application/"/>.
    /// </remarks>
    public class QsCompiler : ICompiler
    {
        private readonly ILogger<QsCompiler> logger;
        private readonly string filename = $"__{UniqueId.CreateUniqueId()}__.qs";
        private readonly List<FilesEmittedArgs> eventQueue = new List<FilesEmittedArgs>();

        /// <summary>Initializes a new instance of the <see cref="QsCompiler"/> class.</summary>
        /// <param name="logger">A <see cref="Logger"/> instance to log compilation messages with.</param>
        public QsCompiler(ILogger<QsCompiler> logger) => this.logger = logger;

        /// <inheritdoc/>
        public event EventHandler<string>? OnDiagnostics;

        /// <inheritdoc/>
        public event EventHandler<QsCompilation>? OnCompilation;

        /// <inheritdoc/>
        public event EventHandler<(int, string)>? OnOutput;

        /// <inheritdoc/>
        public QsCompilation? Compilation { get; private set; }

        /// <inheritdoc/>
        public async Task Compile(string qsharpCode, bool execute = false)
        {
            // necessary references to compile our Q# program
            IEnumerable<string> qsharpReferences = new[]
            {
                "Microsoft.Quantum.QSharp.Core",
                "Microsoft.Quantum.Runtime.Core",
            }.Select(x => Assembly.Load(new AssemblyName(x))).Select(a => a.Location);

            // events emitted by the Q# compiler
            CompilationLoader.CompilationTaskEvent += (sender, args) => { logger.LogDebug($"{args.ParentTaskName} {args.TaskName} - {args.Type}"); };

            // to load our custom rewrite step, we need to point Q# compiler config at our current assembly
            var config = new CompilationLoader.Configuration
            {
                IsExecutable = true,
                RewriteSteps = new List<(string, string?)>
                {
                    (Assembly.GetExecutingAssembly().Location, null),
                },
            };

            void Handler(object? sender, FilesEmittedArgs? args)
            {
                eventQueue.Add(args!);
            }

            eventQueue.Clear();

            InMemoryEmitter.FilesGenerated += Handler;

            // compile Q# code
            var compilationLoader = new CompilationLoader(
                _ => new Dictionary<Uri, string> { { new Uri(Path.GetFullPath(filename)), qsharpCode } }
                   .ToImmutableDictionary(),
                qsharpReferences,
                config,
                new ConsoleLogger(logger));

            InMemoryEmitter.FilesGenerated -= Handler;

            // print any diagnostics
            var diags = compilationLoader.LoadDiagnostics;
            if (diags.Any())
            {
                var diagnostics = string.Join(Environment.NewLine, diags.Select(d => $"{d.Severity} {d.Code} {d.Message}"));

                OnDiagnostics?.Invoke(this, diagnostics);

                // if there are any errors, exit
                if (diags.Any(d => d.Severity == Microsoft.VisualStudio.LanguageServer.Protocol.DiagnosticSeverity.Error))
                {
                    return;
                }
            }

            // communicate that the Q# compilation was successful
            OnCompilation?.Invoke(this, compilationLoader.CompilationOutput);
            Compilation = compilationLoader.CompilationOutput;

            if (!execute)
            {
                return;
            }

            // necessary references to compile C# simulation of the Q# compilation
            IEnumerable<string> csharpReferences = new[]
            {
                "Microsoft.Quantum.QSharp.Core",
                "Microsoft.Quantum.Runtime.Core",
                "Microsoft.Quantum.Simulators",
                "Microsoft.Quantum.EntryPointDriver",
                "System.CommandLine",
                "System.Runtime",
                "netstandard",
                "System.Collections.Immutable",
                typeof(object).Assembly.FullName,
            }.Select(x => Assembly.Load(new AssemblyName(x!))).Select(a => a.Location);

            // find our generated files
            Dictionary<string, string>? generatedFiles = null;
            foreach (var args in eventQueue)
            {
                if (args.CompilationHash == compilationLoader.CompilationOutput.GetHashCode())
                {
                    generatedFiles = args.FileContents;
                    break;
                }
            }

            if (generatedFiles == null)
            {
                logger.LogError("Couldn't find generated files in the event queue");
                return;
            }

            // we captured the emitted C# syntax trees into a static variable in the rewrite step
            IEnumerable<SyntaxTree> syntaxTrees = generatedFiles.Select(x => CSharpSyntaxTree.ParseText(x.Value));

            // compile C# code
            // make sure to pass in the C# references as Roslyn's metadata references
            CSharpCompilation csharpCompilation = CSharpCompilation.Create("qsharp-assembly", syntaxTrees)
                                                                   .WithReferences(csharpReferences.Select(x => MetadataReference.CreateFromFile(x)));

            // print any diagnostics
            List<Diagnostic> csharpDiagnostics = csharpCompilation.GetDiagnostics()
                                                                  .Where(d => d.Severity != DiagnosticSeverity.Hidden && d.Id != "CS1702").ToList();
            if (csharpDiagnostics.Any())
            {
                string? diagnostics = "C# Diagnostics:" + Environment.NewLine +
                                      string.Join(Environment.NewLine, csharpDiagnostics.Select(d => $"{d.Severity} {d.Id} {d.GetMessage()}"));

                OnDiagnostics?.Invoke(this, diagnostics);

                // if there are any errors, exit
                if (csharpDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    return;
                }
            }

            // emit C# code into an in memory assembly
            await using var peStream = new MemoryStream();
            csharpCompilation.Emit(peStream);
            peStream.Position = 0;
            var qsharpLoadContext = new QSharpLoadContext();

            // run the assembly using reflection
            Assembly qsharpAssembly = qsharpLoadContext.LoadFromStream(peStream);

            // the entry point has a special name "__QsEntryPoint__"
            MethodInfo? entryPoint = qsharpAssembly.GetTypes().First(x => x.Name == "__QsEntryPoint__")
                                                   .GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);

            if (entryPoint?.Invoke(null, new object?[] { null }) is Task<int> entryPointTask)
            {
                var sb = new StringBuilder();
                var writer = new StringWriter(sb);

                // intercept the standard output
                TextWriter stdOut = Console.Out;
                Console.SetOut(writer); // TODO: Fix race condition

                // run the program
                int retStatus = await entryPointTask;

                Console.SetOut(stdOut);
                OnOutput?.Invoke(this, (retStatus, sb.ToString()));
            }

            qsharpLoadContext.Unload();
        }
    }
}
