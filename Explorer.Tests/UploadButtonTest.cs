using System;
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
    public class UploadButtonTest
    {
        [Test]
        public void Renders()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();

            // Act
            var ub = ctx.RenderComponent<UploadButton>();

            // Assert
            Assert.AreEqual("Upload", ub.Find("button").TextContent, "Button's name should be rendered");
        }
    }
}
