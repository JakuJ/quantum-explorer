using Bunit;
using Bunit.TestDoubles;
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
            ctx.Services.AddMockJSRuntime();
            var tree = new PanelTree(PanelTree.Alignment.Horizontal);
            var page = RenderTree(ctx, tree);
            page.MarkupMatches("<div class=\"resizable\"></div>", "There should be no components");
        }

        [Test]
        public void RendersASingleComponent()
        {
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var tree = new PanelTree(PanelTree.Alignment.Horizontal);
            tree.AddPanel<Editor>();
            var page = RenderTree(ctx, tree);
            Assert.DoesNotThrow(() => page.FindComponent<Editor>(), "There should be an Editor on the page");
        }

        [Test]
        public void RendersANestedStructure()
        {
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var tree = new PanelTree(PanelTree.Alignment.Horizontal);
            tree.AddPanel<Editor>();

            var two = new PanelTree(PanelTree.Alignment.Vertical);
            tree.AddPanel<Compositor>();
            tree.AddPanel<Visualizer>();

            tree.AddPanel(two);

            var three = new PanelTree(PanelTree.Alignment.Horizontal);
            tree.AddPanel<Visualizer>();
            tree.AddPanel<Compositor>();

            var four = new PanelTree(PanelTree.Alignment.Vertical);
            tree.AddPanel<Visualizer>();
            tree.AddPanel<Visualizer>();

            three.AddPanel(four);
            tree.AddPanel(three);

            var page = RenderTree(ctx, tree);

            Assert.DoesNotThrow(() => page.FindComponent<Editor>(), "There should be an editor on the page");
            Assert.AreEqual(2, page.FindComponents<Compositor>().Count, "There should be two compositors on the page");
            Assert.AreEqual(4, page.FindComponents<Visualizer>().Count, "There should be four visualizers on the page");
        }

        [Test]
        public void AllowsAccessToTheRenderedComponentInstance()
        {
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var tree = new PanelTree(PanelTree.Alignment.Horizontal);

            Panel<Editor> editor = tree.AddPanel<Editor>();
            var page = RenderTree(ctx, tree);

            var editorInstance = page.FindComponent<Editor>().Instance;
            Assert.AreSame(editor.Component, editorInstance, "The instance of the component on the page should be accessible from code");
        }
    }
}
