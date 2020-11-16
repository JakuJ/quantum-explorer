using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles.JSInterop;
using Compiler;
using Explorer.Components.Composer;
using NUnit.Framework;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class ComposerTest
    {
        [Test]
        public async Task RendersComposer()
        {
            // Arrange
            using var ctx = new Bunit.TestContext();
            ctx.Services.AddMockJSRuntime();
            var cut = ctx.RenderComponent<Composer>();

            var grid = new GateGrid();
            grid.AddGate(0, new QuantumGate("H"));
            grid.AddGate(1, new QuantumGate("MResetZ"));
            var ast = new Dictionary<string, GateGrid> { { "tab1", grid } };

            // Act
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGrids(ast));

            // Assert
            cut.Find(".gate-name").TextContent.MarkupMatches("H");

            // it doesn't work, the paths are different for unit testing so the photo cannot be loaded
            //cut.Find(".img").MarkupMatches("MResetZ");
            return;
        }
    }
}
