using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class ComposerTest
    {
        [Test]
        public async Task RendersComposer()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Composer>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grids>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Gate>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Cell>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<SnapPoint>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);

            GateGrid grid = new();
            grid.AddGate(0, new QuantumGate("H"));
            grid.AddGate(1, new QuantumGate("MResetZ"));
            grid.AddGate(2, new QuantumGate("X"));
            grid.AddGate(3, CustomGateFactory.MakeCustomGate("__control__"));
            grid.AddGate(4, new QuantumGate("ResetAll"));

            GateGrid grid21 = new();
            grid21.AddGate(0, new QuantumGate("H"));

            GateGrid grid22 = new();
            grid22.AddGate(0, new QuantumGate("X"));

            Dictionary<string, List<GateGrid>> astTab = new()
            {
                { "TabWithoutNamespace", new List<GateGrid> { grid } },
            };
            Dictionary<string, List<GateGrid>> astTabs = new()
            {
                { "ExampleNamespace.Tab1", new List<GateGrid> { grid } },
                { "ExampleNamespace.Tab2", new List<GateGrid> { grid21, grid22 } },
            };

            // Act
            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(astTab));
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(astTabs));

            // Assert
            // Check if the Grid component is rendered
            IRenderedComponent<Grid> gridComponent = cut.FindComponent<Grid>();

            // Check if the gate name is displayed
            gridComponent.Find(".gate-name").TextContent.MarkupMatches("H");

            // Check if the icon is displayed
            gridComponent.Find("#Reset_Gate");
        }

        [Test]
        public async Task RendersComposerDifferentNamespaces()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Composer>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grids>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Gate>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Cell>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<SnapPoint>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);

            GateGrid grid = new();
            grid.AddGate(0, new QuantumGate("H"));
            grid.AddGate(1, new QuantumGate("MResetZ"));
            grid.AddGate(2, new QuantumGate("X"));
            grid.AddGate(3, CustomGateFactory.MakeCustomGate("__control__"));
            grid.AddGate(4, new QuantumGate("ResetAll"));

            GateGrid grid21 = new();
            grid21.AddGate(0, new QuantumGate("X"));

            GateGrid grid22 = new();
            grid22.AddGate(0, new QuantumGate("H"));

            Dictionary<string, List<GateGrid>> astTabs = new()
            {
                { "FirstNamespace.Tab1", new List<GateGrid>() { grid } },
                { "SecondNamespace.Tab2", new List<GateGrid>() { grid21, grid22 } },
            };
            
            // Act
            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(astTabs));
            var tabs = cut.FindAll(".nav-link.noselect");
            tabs[1].Click(); // click the second tab

            // Assert
            // Check if the Grid component is rendered
            IRenderedComponent<Grid> gridComponent = cut.FindComponent<Grid>();

            // Check if the gate name is displayed
            Assert.IsNotNull(gridComponent.Find("circle"));
        }

        [Test]
        public void ThrowsInvalidOperation()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Composer>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Gate>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Cell>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<SnapPoint>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);

            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();
            Dictionary<string, List<GateGrid>> astEmpty = new();

            // Act & Assert - check if setting the empty AST throws an exception
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(astEmpty)));
        }

        [Test]
        public void RendersCircle()
        {
            // Arrange
            using TestContext ctx = new();

            // Act
            IRenderedComponent<Circle> cut = ctx.RenderComponent<Circle>(("Fill", "red"));

            // Assert
            Assert.AreEqual(123, cut.Markup.Length);
        }
    }
}
