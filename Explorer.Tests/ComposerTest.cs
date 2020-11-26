using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles.JSInterop;
using Compiler;
using Explorer.Components.Composer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            var repoDir = Directory
                .GetParent(Directory.GetCurrentDirectory())
                .Parent.Parent.Parent.FullName;
            var wwwRoot = Path.Join(repoDir, "Explorer", "wwwroot");

            var mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment
                .Setup(m => m.ApplicationName)
                .Returns("ComposerTest");
            mockEnvironment
                .Setup(m => m.EnvironmentName)
                .Returns("UnitTestEnvironment");
            mockEnvironment
                .Setup(m => m.WebRootPath)
                .Returns(wwwRoot);

            using var ctx = new Bunit.TestContext();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddSingleton<IWebHostEnvironment>(_ => mockEnvironment.Object);
            var cut = ctx.RenderComponent<Composer>();

            var grid = new GateGrid();
            grid.AddGate(0, new QuantumGate("H"));
            grid.AddGate(1, new QuantumGate("MResetZ"));
            var ast = new Dictionary<string, GateGrid> { { "tab1", grid } };

            // Act
            await cut.InvokeAsync(async () => await cut.Instance.UpdateGrids(ast));

            // Assert
            // Check if the gate name is displayed
            cut.Find(".gate-name").TextContent.MarkupMatches("H");

            // Check if the icon is displayed
            var src = cut.Find("img").Attributes.GetNamedItem("src");
            Assert.AreEqual(src.Value, "images/icons/MResetZ.svg");
        }
    }
}