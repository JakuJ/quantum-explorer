using Bunit;
using Explorer.Components;
using Explorer.Templates;
using NUnit.Framework;
using Index = Explorer.Pages.Index;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class IndexPageTest
    {
        [Test]
        public void Has3Panels()
        {
            using var ctx = new TestContext();
            var index = ctx.RenderComponent<Index>();

            Assert.AreEqual(3, index.FindComponents<Resizable>().Count, "There should be three Resizable panels at the beginning");
        }

        [Test]
        public void HasExactlyOneEditorCompositorAndVisualizer()
        {
            using var ctx = new TestContext();
            var index = ctx.RenderComponent<Index>();

            Assert.AreEqual(1, index.FindComponents<Editor>().Count, "There should be only one Editor component visible by default");
            Assert.AreEqual(1, index.FindComponents<Compositor>().Count, "There should be only one Compositor component visible by default");
            Assert.AreEqual(1, index.FindComponents<Visualizer>().Count, "There should be only one Visualizer component visible by default");
        }
    }
}
