using System;
using NUnit.Framework;

namespace DynamoDB.Common.UnitTests
{
    public class TableIndexTests
    {
        [Test]
        public void TableIndexConstructorTest()
        {
            var pk = "PartitionKey";
            var sk = "SortingKey";
            var tableIndex = new DynamoTableIndex(pk, sk);
            Assert.AreEqual(pk, tableIndex.PartitionKey);
            Assert.AreEqual(sk, tableIndex.SortKey);
            Assert.AreEqual($"{pk}_{sk}_index", tableIndex.Index);
        }

        [Test]
        public void TableIndexConstructorWrongTest()
        {
            Assert.Throws<ArgumentException>(() => new DynamoTableIndex("a", "aa"));
            Assert.Throws<ArgumentException>(() => new DynamoTableIndex("aa", "a"));
            Assert.Throws<ArgumentException>(() => new DynamoTableIndex("aa".PadLeft(256, 'b'), "aa"));
            Assert.Throws<ArgumentException>(() => new DynamoTableIndex("aa", "aa".PadLeft(256, 'b')));
        }
    }
}
