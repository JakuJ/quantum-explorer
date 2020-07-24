using System.Threading.Tasks;
using Bunit;
using Explorer.Components;
using NUnit.Framework;
using TestContext = Bunit.TestContext;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class EditorTest
    {
        [Test]
        public async Task ChangingCodePropertyUpdatesText()
        {
            using var ctx = new TestContext();
            var page = ctx.RenderComponent<Editor>();
            Editor editor = page.Instance;

            string text = page.Find("textarea").GetAttribute("value");
            Assert.AreEqual(editor.Code, text, "The value of the textarea should be the initial value of the Code property");

            const string testCode = "Sample code";
            await page.InvokeAsync(() => { editor.Code = testCode; });

            text = page.Find("textarea").GetAttribute("value");
            Assert.AreEqual(testCode, text, "The value of the textarea should be the updated value of the Code property");
        }
    }
}
