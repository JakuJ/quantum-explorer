using Bunit;
using Bunit.TestDoubles.JSInterop;
using Compiler;
using Explorer.Templates;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Index = Explorer.Pages.Index;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class IndexPageTest
    {
        private static TestContext InitializeContext()
        {
            var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.AddSingleton<ICompiler>(new MockICompiler());
            return ctx;
        }

        [Test]
        public void Has3Panels()
        {
            using var ctx = InitializeContext();
            var index = ctx.RenderComponent<Index>();
            Assert.AreEqual(3, index.FindComponents<Resizable>().Count, "There should be three Resizable panels at the beginning");
        }
    }
}
