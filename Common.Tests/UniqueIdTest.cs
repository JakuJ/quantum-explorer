using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Common.Tests
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
    }
}
