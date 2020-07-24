using System.Collections.Generic;
using System.Linq;
using Explorer.Utilities;
using NUnit.Framework;

namespace Explorer.Tests
{
    [TestFixture]
    [Parallelizable]
    public class UniqueIdTest
    {
        [Test]
        public void IsUnique()
        {
            const int howMany = 100;
            IEnumerable<string> ids = Enumerable.Range(1, howMany).Select(_ => UniqueId.CreateUniqueId());
            Assert.AreEqual(howMany, new HashSet<string>(ids).Count, "Unique identifiers should be unique");
        }

        [Test]
        public void StartsWithF()
        {
            const int howMany = 100;
            for (var i = 0; i < howMany; i++)
            {
                string id = UniqueId.CreateUniqueId();
                Assert.AreEqual('f', id[0], "Unique identifiers should begin with the character 'f'");
            }
        }
    }
}
