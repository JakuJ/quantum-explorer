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
        private static TestContext GetTestContext()
        {
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped<CellMenusNotifier>();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Composer>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grids>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Grid>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Gate>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<Cell>>().Object);
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<SnapPoint>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);
            return ctx;
        }

        [Test]
        public async Task RendersComposer()
        {
            // Arrange
            using var ctx = GetTestContext();

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
                { "ExampleNamespace.EmptyTab", new List<GateGrid> { } },
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
            gridComponent.Find("#Measurement_Gate");
        }

        [Test]
        public async Task RendersComposerDifferentNamespaces()
        {
            // Arrange
            using var ctx = GetTestContext();

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
        public void ShowsPlaceholderOnNoGrids()
        {
            // Arrange
            using var ctx = GetTestContext();

            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();
            Dictionary<string, List<GateGrid>> astEmpty = new();

            // Act & Assert - check if setting the empty AST shows the placeholder
            Assert.DoesNotThrowAsync(async () => await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(astEmpty)));
            cut.FindComponent<Placeholder>();
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

        [Test]
        public async Task OpensClosesMenuInteractively()
        {
            // Arrange
            using var ctx = GetTestContext();

            Dictionary<string, List<GateGrid>> astTab = new()
            {
                { "DefaultNamespace", new List<GateGrid> { new GateGrid(1, 1) } },
            };

            // Act
            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(astTab));

            var blankCellButton = cut.Find(".grid-blankcell-button");
            blankCellButton.ContextMenu(); // right-click the button -> show menu to add new gate

            var clickBox = cut.FindAll(".click-box")[0];
            clickBox.Click(); // hide the menu

            // Assert
            // Check if the click box has disappeared
            Assert.Throws<ElementNotFoundException>(() => cut.Find(".click-box"));

            // Check if the Grid component is rendered
            IRenderedComponent<Grid> gridComponent = cut.FindComponent<Grid>();

            // Check if there is a blank cell
            Assert.IsNotNull(gridComponent.Find(".grid-blankcell-button"));
        }

        [Test]
        public async Task AddsGateInteractively()
        {
            // Arrange
            using var ctx = GetTestContext();

            Dictionary<string, List<GateGrid>> astTab = new()
            {
                { "DefaultNamespace", new List<GateGrid> { new(1, 1) } },
            };

            // Act
            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(astTab));

            var blankCellButton = cut.Find(".grid-blankcell-button");
            blankCellButton.ContextMenu(); // right-click the button -> show menu to add new gate

            var addGateLink = cut.FindAll(".gate-menu.dropdown-menu .menu-gate")[1];
            addGateLink.Click(); // add the Pauli-X gate

            // Assert
            // Check if the addGateLink refers to Pauli-X gate
            addGateLink.TextContent.Equals("X");

            // Check if the Grid component is rendered
            IRenderedComponent<Grid> gridComponent = cut.FindComponent<Grid>();

            // Check if the gate icon is displayed
            Assert.IsNotNull(gridComponent.Find("circle"));
        }

        [Test]
        public async Task DeletesGateInteractively()
        {
            // Arrange
            using var ctx = GetTestContext();

            GateGrid grid = new();
            grid.AddGate(0, new QuantumGate("H"));

            Dictionary<string, List<GateGrid>> astTab = new()
            {
                { "DefaultNamespace", new List<GateGrid> { grid } },
            };

            // Act
            IRenderedComponent<Composer> cut = ctx.RenderComponent<Composer>();
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGridsAsync(astTab));

            var hCellButton = cut.Find(".grid-cell .gate");
            hCellButton.ContextMenu(); // right-click the button -> show menu to delete the gate

            var delGateLink = cut.FindAll(".gate-menu.dropdown-menu .dropdown-item")[0];
            delGateLink.Click(); // delete the gate

            // Assert
            // Check if the delGateLink refers to deleting gate
            delGateLink.TextContent.Equals("Delete gate");

            // Check if the Grid component is rendered
            IRenderedComponent<Grid> gridComponent = cut.FindComponent<Grid>();

            // Check if there are no gates left
            Assert.Throws<ElementNotFoundException>(() => gridComponent.Find(".gate"));
        }
    }
}
