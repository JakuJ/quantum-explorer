using Bunit;
using Explorer.Components;
using Explorer.Templates;
using Explorer.Utilities.ComponentTree;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class PanelRendererTest
    {
        private static IRenderedComponent<Resizable> RenderTree(TestContext ctx, PanelTree tree)
        {
            RenderFragment fragment = new PanelRenderer().Render(tree);
            return ctx.RenderComponent<Resizable>(("ChildContent", fragment));
        }

        [Test]
        public void RendersAnEmptyTree()
        {
            using var ctx = new TestContext();
            var tree = new PanelTree(PanelTree.Alignment.Horizontal);
            var page = RenderTree(ctx, tree);
            page.MarkupMatches("<div class=\"resizable\"></div>", "There should be no components");
        }

        [Test]
        public void RendersASingleComponent()
        {
            using var ctx = new TestContext();
            var tree = new PanelTree(PanelTree.Alignment.Horizontal);
            tree.AddPanel(new Editor());
            var page = RenderTree(ctx, tree);
            Assert.DoesNotThrow(() => page.FindComponent<Editor>(), "There should be an Editor on the page");
        }

        [Test]
        public void RendersANestedStructure()
        {
            using var ctx = new TestContext();
            var tree = new PanelTree(PanelTree.Alignment.Horizontal);
            tree.AddPanel(new Editor());

            var two = new PanelTree(PanelTree.Alignment.Vertical);
            two.AddPanel(new Compositor());
            two.AddPanel(new Visualizer());

            tree.AddPanel(two);

            var three = new PanelTree(PanelTree.Alignment.Horizontal);
            three.AddPanel(new Visualizer());
            three.AddPanel(new Compositor());

            var four = new PanelTree(PanelTree.Alignment.Vertical);
            three.AddPanel(new Visualizer());
            three.AddPanel(new Visualizer());

            three.AddPanel(four);
            tree.AddPanel(three);

            var page = RenderTree(ctx, tree);

            Assert.DoesNotThrow(() => page.FindComponent<Editor>(), "There should be an editor on the page");
            Assert.AreEqual(2, page.FindComponents<Compositor>().Count, "There should be two compositors on the page");
            Assert.AreEqual(4, page.FindComponents<Visualizer>().Count, "There should be four visualizers on the page");
        }

        [Test]
        public void AllowsForTheSameComponentInTwoPlaces()
        {
            using var ctx = new TestContext();
            var tree = new PanelTree(PanelTree.Alignment.Horizontal);

            var editor = new Editor();
            tree.AddPanel(editor);
            tree.AddPanel(editor);

            var page = RenderTree(ctx, tree);
            Assert.AreEqual(2, page.FindComponents<Editor>().Count, "There should be two separate editors");
        }
    }
}
