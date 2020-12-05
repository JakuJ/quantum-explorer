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
        [Ignore("TODO: Why doesn't BUnit render the child Gate component, even though it's initialized?")]
        [Test]
        public async Task RendersComposer()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Gate>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Cell>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<SnapPoint>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);

            GateGrid grid = new();
            grid.AddGate(0, new QuantumGate("H"));
            grid.AddGate(1, new QuantumGate("MResetZ"));
            Dictionary<string, GateGrid> ast = new() { { "tab1", grid } };

            // Act
            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(ast));

            // Assert
            // Check if the Grid component is rendered
            IRenderedComponent<Grid> gridComponent = cut.FindComponent<Grid>();

            // Check if the gate name is displayed
            gridComponent.Find(".gate-name").TextContent.MarkupMatches("H");

            // Check if the icon is displayed
            IAttr src = gridComponent.Find("img").Attributes.GetNamedItem("src");
            Assert.AreEqual(src.Value, "images/icons/MResetZ.svg");
        }
    }
}