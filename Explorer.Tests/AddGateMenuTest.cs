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
        [Test]
        public void AddsPauliXGate()
        {
            // Arrange
            using TestContext ctx = new();
            string? temp = "";

            // Act
            IRenderedComponent<AddGateMenu> cut = ctx.RenderComponent<AddGateMenu>(parameters => parameters
                .Add(p => p.ExitMenu, gateName => { temp = gateName; }));

            var item = cut.FindAll(".dropdown-item");
            item[0].Click(); // click the 1. item - Pauli-X

            // Assert
            Assert.AreEqual("X", temp);
        }

        [Test]
        public void AddsPauliYGate()
        {
            // Arrange
            using TestContext ctx = new();
            string? temp = "";

            // Act
            IRenderedComponent<AddGateMenu> cut = ctx.RenderComponent<AddGateMenu>(parameters => parameters
                .Add(p => p.ExitMenu, gateName => { temp = gateName; }));

            var item = cut.FindAll(".dropdown-item");
            item[1].Click(); // click the 2. item - Pauli-Y

            // Assert
            Assert.AreEqual("Y", temp);
        }

        [Test]
        public void AddsPauliZGate()
        {
            // Arrange
            using TestContext ctx = new();
            string? temp = "";

            // Act
            IRenderedComponent<AddGateMenu> cut = ctx.RenderComponent<AddGateMenu>(parameters => parameters
                .Add(p => p.ExitMenu, gateName => { temp = gateName; }));

            var item = cut.FindAll(".dropdown-item");
            item[2].Click(); // click the 3. item - Pauli-Z

            // Assert
            Assert.AreEqual("Z", temp);
        }

        [Test]
        public void AddsHadamardGate()
        {
            // Arrange
            using TestContext ctx = new();
            string? temp = "";

            // Act
            IRenderedComponent<AddGateMenu> cut = ctx.RenderComponent<AddGateMenu>(parameters => parameters
                .Add(p => p.ExitMenu, gateName => { temp = gateName; }));

            var item = cut.FindAll(".dropdown-item");
            item[3].Click(); // click the 4. item - Hadamard

            // Assert
            Assert.AreEqual("H", temp);
        }

        [Test]
        public void AddsMResetXGate()
        {
            // Arrange
            using TestContext ctx = new();
            string? temp = "";

            // Act
            IRenderedComponent<AddGateMenu> cut = ctx.RenderComponent<AddGateMenu>(parameters => parameters
                .Add(p => p.ExitMenu, gateName => { temp = gateName; }));

            var item = cut.FindAll(".dropdown-item");
            item[4].Click(); // click the 5. item - MResetX

            // Assert
            Assert.AreEqual("MResetX", temp);
        }
    }
}
