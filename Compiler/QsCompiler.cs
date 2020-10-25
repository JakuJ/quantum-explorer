using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>Initializes a new instance of the <see cref="QsCompiler"/> class.</summary>
        /// <param name="logger">A <see cref="Logger"/> instance to log compilation messages with.</param>
        public QsCompiler(ILogger<QsCompiler> logger) => this.logger = logger;

        /// <inheritdoc />
        public event EventHandler<OutputReadyArgs>? OutputReady;

        /// <inheritdoc/>
        public QsCompilation? Compilation { get; private set; }

        /// <inheritdoc/>
        public async Task Compile(string code)
        {
            string? diagnostics, output;
            (Compilation, diagnostics, output) = await Simulate(code, logger);
            OnOutputReady(new OutputReadyArgs(diagnostics, output));
        }

        private static async Task<(QsCompilation?, string?, string?)> Simulate(string qsharpCode, ILogger<QsCompiler> logger)
        {
            string? diagnostics = null;
            string? output = null;

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

            // compile Q# code
            var compilationLoader = new CompilationLoader(
                _ => new Dictionary<Uri, string> { { new Uri(Path.GetFullPath("__CODE_SNIPPET__.qs")), qsharpCode } }.ToImmutableDictionary(),
                qsharpReferences,
                config,
                new ConsoleLogger(logger));

            // remember the QsCompilation
            QsCompilation? compilation = compilationLoader.CompilationOutput;

            // print any diagnostics
            if (compilationLoader.LoadDiagnostics.Any())
            {
                diagnostics = string.Join(
                    Environment.NewLine,
                    compilationLoader.LoadDiagnostics.Select(d => $"{d.Severity} {d.Code} {d.Message}"));

                // if there are any errors, exit
                if (compilationLoader.LoadDiagnostics.Any(d => d.Severity == Microsoft.VisualStudio.LanguageServer.Protocol.DiagnosticSeverity.Error))
                {
                    return (null, diagnostics, output);
                }
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

            // we captured the emitted C# syntax trees into a static variable in the rewrite step
            IEnumerable<SyntaxTree> syntaxTrees = InMemoryEmitter.GeneratedFiles.Select(x => CSharpSyntaxTree.ParseText(x.Value));

            // compile C# code
            // make sure to pass in the C# references as Roslyn's metadata references
            CSharpCompilation csharpCompilation = CSharpCompilation.Create("qsharp-assembly", syntaxTrees)
                                                                   .WithReferences(csharpReferences.Select(x => MetadataReference.CreateFromFile(x)));

            // print any diagnostics
            List<Diagnostic> csharpDiagnostics = csharpCompilation.GetDiagnostics().Where(d => d.Severity != DiagnosticSeverity.Hidden).ToList();
            if (csharpDiagnostics.Any())
            {
                logger.LogDebug("C# Diagnostics:" + Environment.NewLine +
                                  string.Join(Environment.NewLine, csharpDiagnostics.Select(d => $"{d.Severity} {d.Id} {d.GetMessage()}")));

                // if there are any errors, exit
                if (csharpDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    return (compilation, diagnostics, output);
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
                // intercept the standard output
                TextWriter? oldOut = Console.Out;

                var sb = new StringBuilder();
                var writer = new StringWriter(sb);
                Console.SetOut(writer);

                // run the program
                await entryPointTask;

                output = sb.ToString();
                Console.SetOut(oldOut);
            }

            qsharpLoadContext.Unload();

            return (compilation, diagnostics, output);
        }

        private void OnOutputReady(OutputReadyArgs args) => OutputReady?.Invoke(this, args);
    }
}
