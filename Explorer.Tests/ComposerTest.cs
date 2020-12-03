using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using Compiler;
using Explorer.Components.Composer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

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
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);
            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();

            var grid = new GateGrid();
            grid.AddGate(0, new QuantumGate("H"));
            grid.AddGate(1, new QuantumGate("MResetZ"));
            var ast = new Dictionary<string, GateGrid> { { "tab1", grid } };

            // Act
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(ast));

            // Assert
            // Check if the gate name is displayed
            cut.Find(".gate-name").TextContent.MarkupMatches("H");

            // Check if the icon is displayed
            IAttr src = cut.Find("img").Attributes.GetNamedItem("src");
            Assert.AreEqual(src.Value, "images/icons/MResetZ.svg");
        }
    }
}
