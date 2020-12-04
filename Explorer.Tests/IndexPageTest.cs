using Bunit;
using Bunit.TestDoubles;
using Compiler;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
            TestContext ctx = new();

            ctx.Services.AddMockJSRuntime();
            ctx.Services.AddSingleton(_ => Mock.Of<ICompiler>());
            ctx.Services.AddSingleton(_ => Mock.Of<IWebHostEnvironment>());
            ctx.Services.AddLogging(builder => builder.AddConsole());

            return ctx;
        }

        [Test]
        public void Has3Panels()
        {
            // Arrange
            using TestContext ctx = InitializeContext();

            // Act
            var index = ctx.RenderComponent<Index>();

            // Assert
            Assert.AreEqual(3, index.FindAll(".split-content").Count, "There should be three split panes at the beginning");
        }
    }
}
