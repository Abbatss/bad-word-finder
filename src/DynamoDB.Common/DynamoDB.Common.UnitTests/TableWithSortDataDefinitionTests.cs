using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using NUnit.Framework;

namespace DynamoDB.Common.UnitTests
{
    public class TableWithSortDataDefinitionTests
    {
        [Test]
        public void TableWithSortDataDefinitionConstructorTest()
        {
            Assert.DoesNotThrow(() => new TableWithSortDataDefinition());
        }

        [Test]
        public void AddIndexesTest()
        {
            var createTableRequest = new CreateTableRequest();
            var definition = new TableWithSortDataDefinition();
            definition.AddIndexes(createTableRequest);
            CollectionAssert.IsNotEmpty(createTableRequest.GlobalSecondaryIndexes);
            Assert.AreEqual(ProjectionType.ALL, createTableRequest.GlobalSecondaryIndexes[0].Projection.ProjectionType);
        }

        [Test]
        public void GetIndexesTest()
        {
            var expected = new List<DynamoTableIndex>() { DynamoTableConstants.SortDataIndex };
            var definition = new TableWithSortDataDefinition();
            CollectionAssert.AreEqual(expected, definition.GetIndexes());
        }

        [Test]
        public void DefaultIndexTest()
        {
            var definition = new TableWithSortDataDefinition();
            Assert.AreEqual(DynamoTableConstants.SortDataIndex, definition.DefaultIndex);
        }
    }
}
