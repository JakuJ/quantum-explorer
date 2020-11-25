using System;
using System.Collections.Generic;
using System.Numerics;
using Bunit;
using Bunit.TestDoubles;
using Compiler;
using Explorer.Components;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class VisualizerTest
    {
        private static readonly Random Random = new(1337);

        [Test]
        public void RendersTabsHeaders()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();

            // Act
            var vis = ctx.RenderComponent<Visualizer>();

            // Assert
            Assert.AreEqual(2, vis.Find("ul").ChildElementCount, "There should be two tabs on the page (Output, State Visualizer)");
        }

        [Test]
        public void InitializesWithFirstTabSelected()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();

            // Act
            var vis = ctx.RenderComponent<Visualizer>();

            // Assert
            Assert.True(vis.Find("ul > li:first-child>a").ClassList.Contains("active"), "First tab should be selected");
            Assert.False(vis.Find("ul > li:nth-child(2)>a").ClassList.Contains("active"), "Second tab should not be selected");
        }

        [Test]
        public void SwitchesToQuantumStatesTab()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();

            // Assert
            Assert.False(vis.Find("ul > li:first-child>a").ClassList.Contains("active"), "First tab should not be selected");
            Assert.True(vis.Find("ul > li:nth-child(2)>a").ClassList.Contains("active"), "Second tab should be selected");
        }

        [Test]
        public void SwitchesToOutputTab()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();

            vis.Find("ul > li:first-child>a").Click();

            // Assert
            Assert.True(vis.Find("ul > li:first-child>a").ClassList.Contains("active"), "First tab should be selected");
            Assert.False(vis.Find("ul > li:nth-child(2)>a").ClassList.Contains("active"), "Second tab should not be selected");
        }

        [Test]
        public void RendersCustomOutput()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            const string testString = "Test output";

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Instance.SetOutput(testString);

            // Assert
            Assert.AreEqual(testString, vis.Find(".vis-output").GetAttribute("value"), "Custom output should be displayed in textarea");
        }

        private static List<(int Idx, Complex Value)> GenerateRandomStates(int length, int zerosCount = 0)
        {
            List<(int Idx, Complex Value)> states = new();

            for (int i = 0; i < length; i++)
            {
                states.Add((i, i < zerosCount ? Complex.Zero : new Complex(Random.NextDouble() * 2 - 1, Random.NextDouble() * 2 - 1)));
            }

            return states;
        }

        private static IEnumerable<OperationState> GenerateSampleStates(IEnumerable<int> sizes)
        {
            List<OperationState> states = new();
            bool addAsChild = false;

            foreach (var size in sizes)
            {
                var operation = new OperationState(new string('A', size))
                {
                    Arguments = GenerateRandomStates(size),
                    Results = GenerateRandomStates(size),
                };

                if (addAsChild)
                {
                    states[^1].AddOperation(operation);
                }
                else
                {
                    states.Add(operation);
                }

                addAsChild = !addAsChild;
            }

            return states;
        }

        [Test]
        public void RenderSmallOperatonTree()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            IEnumerable<OperationState> states = GenerateSampleStates(new[] { 5, 4, 8, 3, 6 });

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();
            vis.Instance.ShowStates(states);

            // Assert
            Assert.AreEqual(3, vis.Find(".vis-gate-panel>*:first-child>ul").ChildElementCount, "Tree with 3 root elements should be renderd");
        }

        [Test]
        public void RenderBigOperatonTree()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();
            IEnumerable<OperationState> states =
                GenerateSampleStates(new[] { 6, 4, 9, 16, 80, 45, 32, 89, 3, 54, 6, 4, 9, 16, 80, 45, 32, 89, 3, 54 });

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();
            vis.Instance.ShowStates(states);

            // Assert
            Assert.AreEqual(10, vis.Find(".vis-gate-panel>*:first-child>ul").ChildElementCount, "Tree with 10 root elements should be renderd");
        }

        [Test]
        public void UpdatesOperatonTree()
        {
            // Arrange
            using TestContext ctx = new();
            ctx.Services.AddMockJSRuntime();

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();
            vis.Instance.ShowStates(GenerateSampleStates(new[] { 4, 7, 4, 3, 6, 2 }));
            vis.Instance.ShowStates(GenerateSampleStates(new[] { 8, 2, 4, 3 }));

            // Assert
            Assert.AreEqual(2, vis.Find(".vis-gate-panel>*:first-child>ul").ChildElementCount, "Tree with 2 root elements should be renderd");
        }

        [Test]
        public void RendersChart()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime(JSRuntimeMockMode.Strict);

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();
            vis.Instance.ShowStates(GenerateSampleStates(new[] { 2 }));

            // Assert
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            Assert.ThrowsAsync<System.AggregateException>(async () => vis.Find(".ui-treenode-content").Click()); // its a workaround, because when chart calls JSInterop it loops
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

            Assert.IsTrue(mockJS.Invocations.ContainsKey("Radzen.createChart"), "JSInterop method \"createChart\" should be called");
        }

        [Test]
        public void RendersChartWithoutZeros()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime(JSRuntimeMockMode.Strict);
            var operation = new OperationState("TestOP")
            {
                Arguments = GenerateRandomStates(9, 5),
                Results = GenerateRandomStates(10, 4),
            };

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();
            vis.Instance.ShowStates(new List<OperationState> { operation });
            try { vis.Find(".ui-treenode-content").Click(); } catch { }
            vis.Find(".form-check-input").Change(new ChangeEventArgs() { Value = false });

            // Assert
            Assert.IsTrue(mockJS.Invocations.ContainsKey("Radzen.createChart"), "JSInterop method \"createChart\" should be called");
        }

        [Test]
        public void SwitchesBetweenArgumentsAndResults()
        {
            // Arrange
            using TestContext ctx = new();
            var mockJS = ctx.Services.AddMockJSRuntime(JSRuntimeMockMode.Strict);
            var operation = new OperationState("TestOP")
            {
                Arguments = GenerateRandomStates(9, 5),
                Results = GenerateRandomStates(10, 4),
            };

            // Act
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();
            vis.Instance.ShowStates(new List<OperationState> { operation });
            try { vis.Find(".ui-treenode-content").Click(); } catch { }
            vis.Find(".vis-setting").Change(new ChangeEventArgs() { Value = "Results" });

            // Assert
            Assert.IsTrue(mockJS.Invocations.ContainsKey("Radzen.createChart"), "JSInterop method \"createChart\" should be called");
        }
    }
}
