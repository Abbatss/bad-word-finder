using System;
using System.Collections.Generic;
using DynamoDB.Common.Pagination;
using NUnit.Framework;

namespace DynamoDB.Common.UnitTests
{
    public class DynamoDbFilteringTests
    {
        [Test]
        public void DynamoDbFiltering_ConstructorTest()
        {
            Assert.DoesNotThrow(() => new DynamoDbFiltering(new List<DynamoTableIndex>() { }, new DynamoTableIndex("PrimaryKey", "key2")));
            Assert.Throws<ArgumentNullException>(() => new DynamoDbFiltering(null, new DynamoTableIndex("PrimaryKey", "key2")));
            Assert.Throws<ArgumentNullException>(() => new DynamoDbFiltering(new List<DynamoTableIndex>() { }, null));
        }

        [Test]
        public void DynamoDbFiltering_ChooseIndexPrimaryKey()
        {
            var expectedIndex = new DynamoTableIndex("key1", "key2", 2);
            var indexes = new List<DynamoTableIndex>() { expectedIndex };
            var dynamoDbFiltering = new DynamoDbFiltering(indexes, new DynamoTableIndex("PrimaryKey", "key2"));
            dynamoDbFiltering.AddFilter("key1", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, "value");
            dynamoDbFiltering.AddFilter("key3", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, "value");
            var actualIndex = dynamoDbFiltering.ChooseIndex();
            Assert.AreEqual(expectedIndex, actualIndex);
        }

        [Test]
        public void DynamoDbFiltering_ChooseIndexSortKey()
        {
            new DynamoTableIndex("key1", "key2", 1);
            var expectedIndex = new DynamoTableIndex("key2", "key3", 2);
            var indexes = new List<DynamoTableIndex>() { expectedIndex };
            var dynamoDbFiltering = new DynamoDbFiltering(indexes, new DynamoTableIndex("PrimaryKey", "key2"));
            dynamoDbFiltering.AddFilter("key1", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, "value");
            dynamoDbFiltering.AddFilter("key3", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, "value");
            var actualIndex = dynamoDbFiltering.ChooseIndex();
            Assert.AreEqual(expectedIndex, actualIndex);
        }

        [Test]
        public void DynamoDbFiltering_ChooseIndexPrimaryKey_OnlyEqual()
        {
            new DynamoTableIndex("key1", "key2", 2);
            var expectedIndex = new DynamoTableIndex("key2", "key3", 1);
            var indexes = new List<DynamoTableIndex>() { expectedIndex };
            var dynamoDbFiltering = new DynamoDbFiltering(indexes, new DynamoTableIndex("PrimaryKey", "key2"));
            dynamoDbFiltering.AddFilter("key1", Amazon.DynamoDBv2.DocumentModel.ScanOperator.BeginsWith, "value");
            dynamoDbFiltering.AddFilter("key3", Amazon.DynamoDBv2.DocumentModel.ScanOperator.BeginsWith, "value");
            var actualIndex = dynamoDbFiltering.ChooseIndex();
            Assert.AreEqual(expectedIndex, actualIndex);
        }
    }
}
