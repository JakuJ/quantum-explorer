using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using Compiler;
using Explorer.Components.Composer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class ComposerTest
    {
        /// <summary>
        /// Renders the composer.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Test]
        public async Task RendersComposer()
        {
            // Arrange
            using var ctx = new Bunit.TestContext();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped<ILogger<Grid>>(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddSingleton<IWebHostEnvironment>(_ => Helpers.GetMockEnvironment().Object);
            var cut = ctx.RenderComponent<Composer>();

            var grid = new GateGrid();
            grid.AddGate(0, new QuantumGate("H"));
            grid.AddGate(1, new QuantumGate("MResetZ"));
            var ast = new Dictionary<string, GateGrid> { { "tab1", grid } };

            // Act
            await cut.InvokeAsync(() => cut.Instance.UpdateGrids(ast));

            // Assert
            // Check if the gate name is displayed
            cut.Find(".gate-name").TextContent.MarkupMatches("H");

            // Check if the icon is displayed
            var src = cut.Find("img").Attributes.GetNamedItem("src");
            Assert.AreEqual(src.Value, "images/icons/MResetZ.svg");
        }
    }
}
