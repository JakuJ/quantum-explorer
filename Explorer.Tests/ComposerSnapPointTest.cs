using Bunit;
using Bunit.TestDoubles;
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
    public class ComposerSnapPointTest
    {
        [Test]
        public void RendersSnapPoint()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.TryAddScoped(_ => new Mock<ILogger<SnapPoint>>().Object);
            ctx.Services.TryAddSingleton(_ => Helpers.GetMockEnvironment().Object);

            // Act
            IRenderedComponent<SnapPoint> cut = ctx.RenderComponent<SnapPoint>(("X", 0), ("Y", 0), ("Half", false), ("Locked", false));

            // Assert
            // Check if the proper SnapPoint is displayed
            cut.MarkupMatches(@"<div id=""_0"" class=""grid-snap  "" style=""left: 10px""></div>");
        }
    }
}
