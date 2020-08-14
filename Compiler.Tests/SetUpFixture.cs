using NUnit.Framework;

namespace Compiler.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        public SetUpFixture()
        {
            // Run the static constructor before any tests happen.
            using var compiler = new QsCompiler();
        }
    }
}
