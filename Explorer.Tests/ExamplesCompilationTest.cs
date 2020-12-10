using System.Collections.Generic;
using System.IO;
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
            bool runs = false;

            compiler.OnOutput += (_, _) => { runs = true; };

            foreach (var codeInfo in codes)
            {
                // Act
                runs = false;
                await compiler.Compile(codeInfo.Code);

                // Assert
                Assert.IsTrue(runs, $"Code from {codeInfo.Name} should compile.");
            }
        }

        private List<(string Name, string Code)> GetExampleCodes(string folderPath)
        {
            List<(string Name, string Code)> codes = new();
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException();
            }

            foreach (string file in Directory.EnumerateFiles(folderPath, "*.qs"))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                string code = File.ReadAllText(file);
                codes.Add((name, code));
            }

            return codes;
        }
    }
}
