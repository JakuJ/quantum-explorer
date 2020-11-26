using Bunit;
using Bunit.TestDoubles.JSInterop;
using Compiler;
using Explorer.Templates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
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
            ctx.Services.TryAddSingleton<IWebHostEnvironment>(_ => Helpers.GetMockEnvironment().Object);

            // Mock the compiler
            ctx.Services.AddSingleton(container => Mock.Of<ICompiler>());

            ctx.Services.AddLogging(builder => builder.AddConsole());
            return ctx;
        }

        [Test]
        public void Has3Panels()
        {
            // Arrange
            using var ctx = InitializeContext();

            // Act
            var index = ctx.RenderComponent<Index>();

            // Assert
            Assert.AreEqual(3, index.FindComponents<Resizable>().Count, "There should be three Resizable panels at the beginning");
        }
    }
}
