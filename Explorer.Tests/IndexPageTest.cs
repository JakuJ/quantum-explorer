using System;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles.JSInterop;
using Compiler;
using Explorer.Templates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Index = Explorer.Pages.Index;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class IndexPageTest
    {
        private const string TestCode = "namespace Test { function Test () : Unit { } }";

        private static string NormalizeWhitespace(string input) => new Regex(@"\s+").Replace(input, " ");

        private static TestContext InitializeContext(bool mockCompiler = false)
        {
            var ctx = new TestContext();
            ctx.Services.AddMockJSRuntime();

            if (mockCompiler)
            {
                ctx.Services.AddSingleton(container => Mock.Of<ICompiler>());
            }
            else
            {
                ctx.Services.AddSingleton<ICompiler>(container => new QsCompiler(container.GetRequiredService<ILogger<QsCompiler>>()));
            }

            ctx.Services.AddLogging(builder => builder.AddConsole());
            return ctx;
        }

        [Test]
        public void Has3Panels()
        {
            // Arrange
            using var ctx = InitializeContext(true);

            // Act
            var index = ctx.RenderComponent<Index>();

            // Assert
            Assert.AreEqual(3, index.FindComponents<Resizable>().Count, "There should be three Resizable panels at the beginning");
        }

        [Test]
        public void PrettyPrintsCode()
        {
            // TODO: Second thoughts – all this should probably be in a behavioural/integration test.

            // Arrange
            using var ctx = InitializeContext();
            var index = ctx.RenderComponent<Index>();
            index.Instance.Editor.Component.Code = TestCode;

            // Act
            Assert.DoesNotThrow(
                () =>
                {
                    var button = index.Find("button#compile");
                    button.Click();
                }, "Clicking on the compile button should not throw");

            index.WaitForState(() => !index.Instance.OutputEditor.Component.Code.StartsWith("The"), TimeSpan.FromSeconds(5));

            // Assert
            Assert.AreEqual(0, index.Instance.Diagnostics.Component.Diagnostics.Count, "There should be no compiler diagnostics");

            Assert.AreEqual(
                NormalizeWhitespace(TestCode),
                NormalizeWhitespace(index.Instance.OutputEditor.Component.Code),
                "Pretty-printed code should be the same as the original.");
        }
    }
}
