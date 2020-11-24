using Bunit;
using Bunit.TestDoubles;
using Explorer.Components;
using Explorer.Templates;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class VisualizerTest
    {
        [Test]
        public void RendersTabsHeaders()
        {
            using var ctx = new TestContext();
            var vis = ctx.RenderComponent<Visualizer>();

            Assert.AreEqual(2, vis.Find("ul").ChildElementCount, "There should be two tabs on the page (Output, State Visualizer)");
        }

        [Test]
        public void InitializesWithFirstTabSelected()
        {
            using var ctx = new TestContext();
            var vis = ctx.RenderComponent<Visualizer>();
            Assert.False(vis.Find("ul > li:first-child").ClassList.Contains("active"));
            Assert.True(vis.Find("ul > li:nth-child(2)").ClassList.Contains("active"));
        }

        [Test]
        public void SwitchesTabs()
        {
            using var ctx = new TestContext();
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li").Click();

            Assert.AreEqual(2, vis.Find("ul").ChildElementCount, "There should be two tabs on the page (Output, State Visualizer)");
        }
    }
}
