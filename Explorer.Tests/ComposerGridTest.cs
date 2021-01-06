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
        [Test]
        public void RendersGridControlled()
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
    }
}
