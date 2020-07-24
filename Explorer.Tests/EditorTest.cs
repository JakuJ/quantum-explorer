using AngleSharp.Dom;
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
        public void ChangingCodePropertyUpdatesText()
        {
            using var ctx = new TestContext();
            var page = ctx.RenderComponent<Editor>();
            Editor instance = page.Instance;

            string text = page.Find("textarea").GetAttribute("value");
            Assert.AreEqual(instance.Code, text, "The value of the textarea should be the initial value of the Code property");

            const string testCode = "Sample code";
            instance.Code = testCode;

            text = page.Find("textarea").GetAttribute("value");
            Assert.AreEqual(testCode, text, "The value of the textarea should be the updated value of the Code property");
        }
    }
}
