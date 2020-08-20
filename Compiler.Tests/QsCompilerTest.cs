using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    [Parallelizable]
    public class QsCompilerTest
    {
        [Test]
        public async Task CompilesExampleCodeWithoutWarnings()
        {
            string code = await GetSource("Library");
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            using var compiler = new QsCompiler(loggerFactory);

            await compiler.Compile(code);
            Assert.AreEqual(0, compiler.GetDiagnostics().Count, "There should be no warnings or errors");
        }

        private static async Task<string> GetSource(string baseName)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $"TestSources/{baseName}.qs");
            return await File.ReadAllTextAsync(path);
        }
    }
}
