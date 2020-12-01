using System.Threading.Tasks;
using Compiler.AzureFunction;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    public class FunctionTest
    {
        [TestCase("HelloWorld")]
        public async Task AzureFunctionCompilesAndExecutesCode(string file)
        {
            // Arrange
            string code = await Helpers.GetSourceFile(file);
            LocalAzureFunctionClient client = new();
            AzureFunctionCompiler compiler = new(client, Helpers.ConsoleLogger);

            // setup event handlers
            compiler.OnOutput += (_, s) => Assert.IsNotEmpty(s, "The returned output shouldn't be empty");
            compiler.OnDiagnostics += (_, s) => Assert.IsNull(s, "There should be no diagnostics");
            compiler.OnGrids += (_, s) => Assert.IsNotNull(s, "GateGrids should be returned");
            compiler.OnStatesRecorded += (_, s) => Assert.IsNotNull(s, "State should be recorded");

            // Act && Assert
            await compiler.Compile(code);
        }
    }
}
