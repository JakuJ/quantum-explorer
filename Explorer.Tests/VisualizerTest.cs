using Bunit;
using Bunit.TestDoubles;
using Compiler;
using Explorer.Components;
using Explorer.Templates;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class VisualizerTest
    {

        private static readonly Random Random = new();

        [Test]
        public void RendersTabsHeaders()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var vis = ctx.RenderComponent<Visualizer>();

            // Assert
            Assert.AreEqual(2, vis.Find("ul").ChildElementCount, "There should be two tabs on the page (Output, State Visualizer)");
        }

        [Test]
        public void InitializesWithFirstTabSelected()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var vis = ctx.RenderComponent<Visualizer>();

            // Assert
            Assert.True(vis.Find("ul > li:first-child>a").ClassList.Contains("active"), "First tab should be selected");
            Assert.False(vis.Find("ul > li:nth-child(2)>a").ClassList.Contains("active"), "Second tab should not be selected");
        }

        [Test]
        public void SwitchesToQuantumStatesTab()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var vis = ctx.RenderComponent<Visualizer>();

            // Act
            vis.Find("ul > li:nth-child(2)>a").Click();

            // Assert
            Assert.False(vis.Find("ul > li:first-child>a").ClassList.Contains("active"), "First tab should not be selected");
            Assert.True(vis.Find("ul > li:nth-child(2)>a").ClassList.Contains("active"), "Second tab should be selected");
        }

        [Test]
        public void SwitchesToOutputTab()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var vis = ctx.RenderComponent<Visualizer>();

            // Act
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
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var vis = ctx.RenderComponent<Visualizer>();
            var testString = "Test output";

            // Act
            vis.Instance.SetText(testString);

            // Assert
            Assert.AreEqual(testString, vis.Find(".vis-output").GetAttribute("value"), "Custom output should be displayed in textarea");
        }

        private List<(int Idx, Complex Value)> GenerateRandomStates(int length, int zerosCount = 0)
        {
            List<(int Idx, Complex Value)> states = new();
            for (int i = 0; i < length; i++)
            {
                if (i < zerosCount)
                {
                    states.Add((i, new Complex(0, 0)));
                }
                else
                {
                    states.Add((i, new Complex(Random.NextDouble() * 2 - 1, Random.NextDouble() * 2 - 1)));
                }
            }

            return states;
        }

        private List<OperationState> GenerateSampleStates(List<int> sizes)
        {
            List<OperationState> states = new();
            bool addAsChild = false;
            foreach (var size in sizes)
            {
                var operation = new OperationState(new string('A', size)) { Arguments = GenerateRandomStates(size), Results = GenerateRandomStates(size) };
                if (addAsChild)
                {
                    states[states.Count - 1].AddOperation(operation);
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
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();
            List<OperationState> states = GenerateSampleStates(new List<int>() { 5, 4, 8, 3, 6 });

            // Act
            vis.Instance.ShowStates(states);

            // Assert
            Assert.AreEqual(3, vis.Find(".vis-gate-panel>*:first-child>ul").ChildElementCount, "Tree with 3 root elements should be renderd");
        }

        [Test]
        public void RenderBigOperatonTree()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();
            List<OperationState> states = GenerateSampleStates(new List<int>() { 6, 4, 9, 16, 80, 45, 32, 89, 3, 54, 6, 4, 9, 16, 80, 45, 32, 89, 3, 54 });

            // Act
            vis.Instance.ShowStates(states);

            // Assert
            Assert.AreEqual(10, vis.Find(".vis-gate-panel>*:first-child>ul").ChildElementCount, "Tree with 10 root elements should be renderd");
        }

        [Test]
        public void UpdatesOperatonTree()
        {
            // Arrange
            using var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();
            var vis = ctx.RenderComponent<Visualizer>();
            vis.Find("ul > li:nth-child(2)>a").Click();

            // Act
            vis.Instance.ShowStates(GenerateSampleStates(new List<int>() { 4, 7, 4, 3, 6, 2 }));

            vis.Instance.ShowStates(GenerateSampleStates(new List<int>() { 8, 2, 4, 3 }));

            // Assert
            Assert.AreEqual(2, vis.Find(".vis-gate-panel>*:first-child>ul").ChildElementCount, "Tree with 2 root elements should be renderd");
        }
    }
}
