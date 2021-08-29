using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace DynamoDB.Common
{
    public interface IDynamoDBTableDefinition
    {
        void AddIndexes(CreateTableRequest createTableRequest);
        DynamoTableIndex DefaultIndex { get; }
        IEnumerable<DynamoTableIndex> GetIndexes();
    }

    public static class DynamoTableConstants
    {
        public const string PrimaryPartitionKey = "Id";
        public const string PrimarySortKey = "SK";
        public const string SortData_SPK = "SortData";
        public static DynamoTableIndex SortDataIndex = new DynamoTableIndex(PrimarySortKey, SortData_SPK);
    }
}
