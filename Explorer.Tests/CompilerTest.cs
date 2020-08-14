using System.IO;
using System.Threading.Tasks;
using Compiler;
using NUnit.Framework;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class CompilerTest
    {
        public async Task<string> GetSource(string baseName)
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, $"/TestSources/{baseName}.qs");
            return await File.ReadAllTextAsync(path);
        }

        [Test]
        public async Task CompilesExampleCode()
        {
        }
    }
}
