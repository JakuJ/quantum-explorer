using Bunit;
using Bunit.TestDoubles;
using Common;
using Explorer.Components.Composer;
using Explorer.Components.Composer.Drawing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class ComposerGridTest
    {
        private static TestContext GetTestContext()
        {
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped<CellMenusNotifier>();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Gate>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Cell>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<SnapPoint>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);
            return ctx;
        }

        [Test]
        public void RendersGridControlled()
        {
            // Arrange
            using var ctx = GetTestContext();

            GateGrid grid = new();
            grid.AddGate(0, 0, new QuantumGate("X"));
            grid.AddGate(0, 1, CustomGateFactory.MakeCustomGate("__control__"));

            // Act
            IRenderedComponent<Grid> cut = ctx.RenderComponent<Grid>(("GateGrid", grid));

            // Assert
            // Check if the Line component is rendered
            IRenderedComponent<Line> lineComponent = cut.FindComponent<Line>();

            // Check if the proper line is displayed
            lineComponent.MarkupMatches(@"<svg height=""40"" width=""60"" class=""composer-svg""style=""left:0px; top: 0px;"">
                <line x1=""0"" y1=""20"" x2=""60"" y2=""20"" class=""composer-line""></line></svg>");
        }

        [Test]
        public void RendersGridSwap()
        {
            // Arrange
            using var ctx = GetTestContext();

            GateGrid grid = new();
            grid.AddGate(0, 0, new QuantumGate("SWAP"));
            grid.AddGate(0, 1, new QuantumGate("SWAP"));
            grid.AddGate(1, new QuantumGate("M"));

            // Act
            IRenderedComponent<Grid> cut = ctx.RenderComponent<Grid>(("GateGrid", grid));

            // Assert
            // Check if the SWAP gates are rendered
            var swapGates = cut.FindAll(".gate-swap");

            // Check if there are two SWAP gates
            Assert.AreEqual(2, swapGates.Count);

            // Check if the SWAP gate is properly displayed
            swapGates[0].MarkupMatches(@"<div class=""gate-swap""><svg height=""24"" width=""24"" class=""composer-svg""
                style=""left:8px; top: 8px;""><line x1=""0"" y1=""0"" x2=""24"" y2=""24"" class=""composer-line""></line></svg>
                <svg height=""24"" width=""24"" class=""composer-svg"" style=""left:8px; top: 8px;"">
                <line x1=""0"" y1=""24"" x2=""24"" y2=""0"" class=""composer-line""></line></svg></div>");
        }
    }
}
