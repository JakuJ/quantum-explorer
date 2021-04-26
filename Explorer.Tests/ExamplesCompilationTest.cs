using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Compiler;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Explorer.Tests
{
    [Parallelizable]
    [TestFixture]
    public class ExamplesCompilationTest
    {
        [TestCase("./CodeExamples/")]
        public async Task ExamplesCompile(string directoryPath)
        {
            // Arrange
            var codes = GetExampleCodes(directoryPath);
            QsCompiler compiler = new(Mock.Of<ILogger>());
            var runs = false;

            compiler.OnOutput += (_, _) => { runs = true; };

            foreach ((string name, string code) in codes)
            {
                // Act
                runs = false;
                await compiler.Compile(code);

                // Assert
                Assert.IsTrue(runs, $"Code from {name} should compile.");
            }
        }

        private static IEnumerable<(string Name, string Code)> GetExampleCodes(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException();
            }

            return from file in Directory.EnumerateFiles(folderPath, "*.qs")
                   let code = File.ReadAllText(file)
                   select (file, code);
        }
    }
}
