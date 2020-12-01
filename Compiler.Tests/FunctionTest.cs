using System.Threading.Tasks;
using Compiler.AzureFunction;
using Compiler.AzureFunction.Connection;
using Moq;
using NUnit.Framework;

namespace Compiler.Tests
{
    [TestFixture]
    public class FunctionTest
    {
        [TestCase("HelloWorld")]
        public async Task CompilesAndExecutesCode(string file)
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

        public async Task ReturnsDiagnosticsFromInvalidCode(string file)
        {
            // Arrange
            const string code = "namespace test { function }"; // invalid code
            LocalAzureFunctionClient client = new();
            AzureFunctionCompiler compiler = new(client, Helpers.ConsoleLogger);

            // setup event handlers
            compiler.OnDiagnostics += (_, s) => Assert.IsNotEmpty(s, "There should be some diagnostics");
            compiler.OnOutput += (_, s) => Assert.Fail($"There should be no output. Got: {s}");
            compiler.OnGrids += (_, s) => Assert.Fail($"There should be no Grids. Got: {s}");
            compiler.OnStatesRecorded += (_, s) => Assert.Fail($"There should be no states recorded. Got: {s}");

            // Act && Assert
            await compiler.Compile(code);
        }

        [TestCase(null)]
        [TestCase("Obviously not valid JSON")]
        public async Task HandlesInvalidResponses(string? response)
        {
            // Arrange
            Mock<IHttpClient> mockClient = new();
            mockClient.Setup(x => x.MakeRequest(It.IsAny<string>())).Returns(Task.FromResult(response));

            AzureFunctionCompiler compiler = new(mockClient.Object, Helpers.ConsoleLogger);

            // setup event handlers
            compiler.OnDiagnostics += (_, s) => Assert.AreEqual(
                "There has been an issue while processing your request.\nTry again later.",
                s,
                "There should be a diagnostic message");

            compiler.OnOutput += (_, s) => Assert.Fail($"There should be no output. Got: {s}");
            compiler.OnGrids += (_, s) => Assert.Fail($"There should be no Grids. Got: {s}");
            compiler.OnStatesRecorded += (_, s) => Assert.Fail($"There should be no states recorded. Got: {s}");

            // Act && Assert
            await compiler.Compile("Some code");
        }
    }
}
