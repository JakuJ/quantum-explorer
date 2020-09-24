using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Compiler.Tests
{
    /// <summary>
    /// Helper classes used in this test project.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// A simple console logger to be used in the test suite.
        /// </summary>
        public static readonly ILogger<QsCompiler> ConsoleLogger
            = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<QsCompiler>();

        /// <summary>
        /// Read the contents of a Q# source file in the TestSources directory.
        /// </summary>
        /// <param name="baseName">The name of the file without the extension.</param>
        /// <returns>The contents of the file.</returns>
        public static async Task<string> GetSourceFile(string baseName)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $"TestSources/{baseName}.qs");
            return await File.ReadAllTextAsync(path);
        }
    }
}
