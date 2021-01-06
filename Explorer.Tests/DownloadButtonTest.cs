using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using Explorer.Components;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [Parallelizable]
    [TestFixture]
    public class DownloadButtonTest
    {
        [Test]
        public void Renders()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();

            // Act
            var dwnlB = ctx.RenderComponent<DownloadButton>();

            // Assert
            Assert.AreEqual("Download", dwnlB.Find("button").TextContent, "Button's name should be rendered");
        }

        [Test]
        public void DoesNothingOnClickWhenNoParameters()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJs = ctx.Services.AddMockJSRuntime();
            var dwnlB = ctx.RenderComponent<DownloadButton>();

            // Act
            dwnlB.Find("button").Click();

            // Assert
            Assert.IsEmpty(mockJs.Invocations, "No download call to JSRuntime should be made");
        }

        [Test]
        public void CallsJSRuntimeWhenClicked()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJs = ctx.Services.AddMockJSRuntime();
            var sampleCode = "testCode";
            var dwnlB = ctx.RenderComponent<DownloadButton>(("GetCode", (Func<Task<string>>)(() => Task.FromResult(sampleCode))));

            // Act
            dwnlB.Find("button").Click();

            // Assert
            Assert.IsTrue(mockJs.Invocations.ContainsKey("Library.saveTextFile"), "Call to JSRuntime to download code should be made");
            Assert.AreEqual(sampleCode, mockJs.Invocations["Library.saveTextFile"].FirstOrDefault().Arguments[1]);
        }
    }
}
