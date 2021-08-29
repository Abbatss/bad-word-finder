using System.Collections.Generic;
using Amazon.DynamoDBv2.DocumentModel;
using DynamoDB.Common.Pagination;
using NUnit.Framework;

namespace DynamoDB.Common.UnitTests
{
    public class BuilderPaginationTokenTests
    {
        [Test]
        public void BuilderJsonPaginationToken_Build_Test()
        {
            var builderPaginationToken = new JsonPaginationTokenBuilder().Add("SK", "Template");
            Assert.AreEqual("{\"SK\":{\"S\":\"Template\"}}", builderPaginationToken.Build());
        }

        [Test]
        public void DirectorJsonPaginationFilterToken_IndexTheSame_Make_Test()
        {
            string secondaryPartitionKey = "PrimarySchemaId";
            string secondarySortKey = "SortData";
            string filterKey = "SK";

            var tableIndex = new DynamoTableIndex(secondaryPartitionKey, secondarySortKey);
            var dbEntryPrimaryPartition = new Primitive("identificator");
            var dbEntrySecondaryPartition = new Primitive("SchemaId");
            var dbEntrySecondarySort = new Primitive("Name");
            var dbEntryFilter = new Primitive("Template");

            var document = new Dictionary<string, DynamoDBEntry>
            {
                { DynamoTableConstants.PrimaryPartitionKey, dbEntryPrimaryPartition },
                { tableIndex.PartitionKey, dbEntrySecondaryPartition },
                { tableIndex.SortKey, dbEntrySecondarySort },
                { filterKey, dbEntryFilter },
            };

            var director = new JsonPaginationTokenFilterDirector(DynamoTableConstants.SortDataIndex, document);
            Assert.AreEqual("{\"SK\":{\"S\":\"Template\"},\"SortData\":{\"S\":\"Name\"},\"Id\":{\"S\":\"identificator\"}}", director.Make().Build());
        }

        [Test]
        public void DirectorJsonPaginationFilterToken_Make_Test()
        {
            string secondaryPartitionKey = "PrimarySchemaId";
            string secondarySortKey = "SortData";
            string filterKey = "SK";

            var tableIndex = new DynamoTableIndex(secondaryPartitionKey, secondarySortKey);
            var dbEntryPrimaryPartition = new Primitive("identificator");
            var dbEntrySecondaryPartition = new Primitive("SchemaId");
            var dbEntrySecondarySort = new Primitive("Name");
            var dbEntryFilter = new Primitive("Template");

            var document = new Dictionary<string, DynamoDBEntry>
            {
                { DynamoTableConstants.PrimaryPartitionKey, dbEntryPrimaryPartition },
                { tableIndex.PartitionKey, dbEntrySecondaryPartition },
                { tableIndex.SortKey, dbEntrySecondarySort },
                { filterKey, dbEntryFilter },
            };

            var director = new JsonPaginationTokenFilterDirector(tableIndex, document);
            Assert.AreEqual("{\"PrimarySchemaId\":{\"S\":\"SchemaId\"},\"SortData\":{\"S\":\"Name\"},\"Id\":{\"S\":\"identificator\"},\"SK\":{\"S\":\"Template\"}}", director.Make().Build());
        }
    }
}
