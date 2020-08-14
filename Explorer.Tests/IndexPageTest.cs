using System.Diagnostics.CodeAnalysis;
using Bunit;
using Bunit.TestDoubles.JSInterop;
using Compiler;
using Explorer.Components;
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
        [Test]
        public void Has3Panels()
        {
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            ctx.Services.AddSingleton<ICompiler>(new QsCompiler());

            var index = ctx.RenderComponent<Index>();
            Assert.AreEqual(3, index.FindComponents<Resizable>().Count, "There should be three Resizable panels at the beginning");
        }
    }
}
