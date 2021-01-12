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
        [TestCase("X", 0)]
        [TestCase("Y", 1)]
        [TestCase("Z", 2)]
        [TestCase("H", 3)]
        [TestCase("__control__", 4)]
        [TestCase("MResetZ", 5)]
        public void AddsGate(string name, int index)
        {
            // Arrange
            using TestContext ctx = new();
            var temp = "";

            // Act
            IRenderedComponent<AddGateMenu> cut = ctx.RenderComponent<AddGateMenu>(parameters => parameters
                                                                                      .Add(p => p.ExitMenu, gateName => { temp = gateName; }));

            IRefreshableElementCollection<IElement> item = cut.FindAll(".dropdown-item");
            item[index].Click();

            // Assert
            Assert.AreEqual(name, temp, $"Gate with name {name} is expected after clicking menu item {index}");
        }
    }
}
