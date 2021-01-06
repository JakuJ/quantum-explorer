using System.Collections.Generic;
using Bunit;
using Bunit.TestDoubles;
using Common;
using Explorer.Components.Composer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class ComposerGridsTest
    {
        [Test]
        public void RendersComposerGrids()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grids>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Gate>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Cell>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<SnapPoint>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);

            GateGrid grid21 = new();
            grid21.AddGate(0, new QuantumGate("X"));

            GateGrid grid22 = new();
            grid22.AddGate(0, new QuantumGate("H"));

            var gridList = new List<GateGrid>() { grid21, grid22 };

            // Act
            IRenderedComponent<Grids> cut = ctx.RenderComponent<Grids>(("GridList", gridList));
            var tabs = cut.FindAll(".nav-link");
            tabs[1].Click(); // click the second tab

            // Assert
            // Check if the Grid component is rendered
            IRenderedComponent<Grid> gridComponent = cut.FindComponent<Grid>();

            // Check if the gate name is displayed
            gridComponent.Find(".gate-name").TextContent.MarkupMatches("H");
        }
    }
}
