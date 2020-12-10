using System;
using System.Linq;
using Bunit;
using Bunit.TestDoubles;
using Explorer.Components;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [Parallelizable]
    [TestFixture]
    public class ExamplesDropdownTest
    {
        private const string FilesDirectory = "./TestExampleCodes";

        [Test]
        public void Renders()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();

            // Act
            var ed = ctx.RenderComponent<ExamplesDropdown>();

            // Assert
            Assert.IsTrue(ed.Find("#dropdownMenuButton").TextContent.Contains("Examples"), "Button should contain expected text");
        }

        [Test]
        public void DropdownShowsOnClick()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            var ed = ctx.RenderComponent<ExamplesDropdown>();

            // Act
            ed.Find("#dropdownMenuButton").Click();

            // Assert
            Assert.AreEqual(1, ed.FindAll(".dropdown-menu").Count, "One dropdown menu should be rendered");
        }

        [Test]
        public void DropdownEmptyWhenWrongExamplesPath()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            var ed = ctx.RenderComponent<ExamplesDropdown>(("ExamplesFolderPath", "./nonexistentFolder/"));

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
            ctx.Services.AddMockJSRuntime();

            var ed = ctx.RenderComponent<ExamplesDropdown>(("ExamplesFolderPath", FilesDirectory));

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
            ctx.Services.AddMockJSRuntime();

            string selectedString = "";
            var ed = ctx.RenderComponent<ExamplesDropdown>(
                ("ExamplesFolderPath", FilesDirectory),
                ("OnSelected", (Action<string>)(s => selectedString = s)));

            // Act
            ed.Find("#dropdownMenuButton").Click();
            ed.FindAll(".dropdown-menu>a").First(a => a.TextContent == "testFile1").Click();

            // Assert
            Assert.IsTrue(selectedString.StartsWith("testCode1"), "Name test1 should be displayed");
        }
    }
}
