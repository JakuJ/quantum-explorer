using System.Threading.Tasks;
using Bunit;
using Explorer.Components;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class ProgressTest
    {
        [Test]
        public async Task RendersProgressBar()
        {
            // Arrange
            using TestContext ctx = new();

            // Act
            IRenderedComponent<Progress> cut = ctx.RenderComponent<Progress>();
            await cut.InvokeAsync(() => cut.Instance.Running = true);

            // Assert
            Assert.AreEqual(cut.Markup.Length, 130);
        }
    }
}
