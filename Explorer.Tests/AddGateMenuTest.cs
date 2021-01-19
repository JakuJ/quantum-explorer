using AngleSharp.Dom;
using Bunit;
using Explorer.Components.Composer;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class AddGateMenuTest
    {
        [TestCase("__control__", 0)]
        [TestCase("X", 1)]
        [TestCase("Y", 2)]
        [TestCase("Z", 3)]
        [TestCase("SWAP", 4)]
        [TestCase("H", 5)]
        [TestCase("S", 6)]
        [TestCase("T", 7)]
        [TestCase("I", 8)]
        [TestCase("MResetX", 9)]
        [TestCase("MResetY", 10)]
        [TestCase("MResetZ", 11)]
        public void AddsGate(string name, int index)
        {
            // Arrange
            using TestContext ctx = new();
            var temp = "";

            // Act
            IRenderedComponent<AddGateMenu> cut = ctx.RenderComponent<AddGateMenu>(parameters => parameters
                                                                                      .Add(p => p.ExitMenu, gateName => { temp = gateName; }));

            IRefreshableElementCollection<IElement> item = cut.FindAll(".menu-gate");
            item[index].Click();

            // Assert
            Assert.AreEqual(name, temp, $"Gate with name {name} is expected after clicking menu item {index}");
        }
    }
}
