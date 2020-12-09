using System;
using System.Linq;
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
    public class ExamplesDropdownTest
    {
        private string filesDirectory = "./TestExampleCodes";

        [Test]
        public void Renders()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();

            // Act
            var ed = ctx.RenderComponent<ExamplesDropdown>();

            // Assert
            Assert.AreEqual("Examples", ed.Find("#dropdownMenuButton").TextContent, "Button should contain expected text");
        }

        [Test]
        public void DropdownShowsOnClick()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();
            var ed = ctx.RenderComponent<ExamplesDropdown>();

            // Act
            ed.Find("#dropdownMenuButton").Click();

            // Assert
            Assert.AreEqual(1, ed.FindAll(".dropdown-menu").Count, "One dropdown menu should be rendered");
            Assert.AreEqual(1, ed.FindAll(".spinner-border").Count, "One spinner should be rendered, when no parameters");
        }

        [Test]
        public void DropdownEmptyWhenWrongExamplesPath()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();
            var ed = ctx.RenderComponent<ExamplesDropdown>(("ExamplesFolderPath", "./nonexistingFolder/"));

            // Act
            ed.Find("#dropdownMenuButton").Click();

            // Assert
            Assert.AreEqual(0, ed.FindAll(".dropdown-menu>a").Count, "No items should be shown in dropdown");
        }

        [Test]
        public void DropdownRendersCodeNames()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();

            var ed = ctx.RenderComponent<ExamplesDropdown>(("ExamplesFolderPath", filesDirectory));

            // Act
            ed.Find("#dropdownMenuButton").Click();

            // Assert
            Assert.AreEqual(2, ed.FindAll(".dropdown-menu>a").Count, "2 items should be shown in dropdown");

            var printedNames = ed.Find(".dropdown-menu").Children.Select(a => a.TextContent).ToList();

            Assert.Contains("test file2", printedNames, "Name test1 should be displayed");
            Assert.Contains("testFile1", printedNames, "Name test2 should be displayed");
        }

        [Test]
        public void InvokesOnSelectMethod()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime();

            string selectedString = "";
            Action<string> onSelect = s => selectedString = s;
            var ed = ctx.RenderComponent<ExamplesDropdown>(("ExamplesFolderPath", filesDirectory), ("OnSelected", onSelect));

            // Act
            ed.Find("#dropdownMenuButton").Click();
            ed.FindAll(".dropdown-menu>a").Where(a => a.TextContent == "testFile1").First().Click();

            // Assert
            Assert.IsTrue(selectedString.StartsWith("testCode1"), "Name test1 should be displayed");
        }
    }
}
