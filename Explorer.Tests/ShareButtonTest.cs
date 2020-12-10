using System;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Bunit.TestDoubles;
using DatabaseHandler;
using Explorer.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [Parallelizable]
    [TestFixture]
    public class ShareButtonTest
    {
        [Test]
        public void Renders()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();
            var dbhMock = new Mock<ICodeDatabaseHandler>();
            ctx.Services.AddSingleton(dbhMock.Object);

            // Act
            var sb = ctx.RenderComponent<ShareButton>();

            // Assert
            Assert.AreEqual("Share", sb.Find("button").TextContent, "Button's name should be rendered");
        }

        [Test]
        public void DoesNothingOnClickWhenNoParameters()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();
            var dbhMock = new Mock<ICodeDatabaseHandler>();
            ctx.Services.AddSingleton(dbhMock.Object);
            var sb = ctx.RenderComponent<ShareButton>();

            // Act
            sb.Find("button").Click();

            // Assert
            Assert.IsEmpty(mockJS.Invocations, "No popup initialize calls should be made");
        }

        [Test]
        public void OpenPopupOnClick()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();
            var dbhMock = new Mock<ICodeDatabaseHandler>();
            ctx.Services.AddSingleton(dbhMock.Object);
            Func<Task<string>> getCode = () => Task.FromResult("codeExample");
            var sb = ctx.RenderComponent<ShareButton>(("GetCode", getCode));

            // Act
            sb.Find("button").Click();

            // Assert
            Assert.IsTrue(mockJS.Invocations.ContainsKey("Library.showSharePopOver"), "Call to create popup should be made");
            dbhMock.Verify(m => m.SaveCode("", "codeExample"), Times.Once());
        }

        [Test]
        public void OnlyOneDBCallWithNoCodeChange()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();
            var dbhMock = new Mock<ICodeDatabaseHandler>();
            dbhMock.Setup(m => m.SaveCode("", "codeExample")).Returns(Guid.NewGuid());
            ctx.Services.AddSingleton(dbhMock.Object);
            Func<Task<string>> getCode = () => Task.FromResult("codeExample");
            var sb = ctx.RenderComponent<ShareButton>(("GetCode", getCode));

            // Act
            sb.Find("button").Click();

            sb.Find("button").Click();

            // Assert
            dbhMock.Verify(m => m.SaveCode("", "codeExample"), Times.Once());
        }
    }
}
