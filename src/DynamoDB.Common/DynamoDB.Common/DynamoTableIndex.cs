using System;

namespace DynamoDB.Common
{
    public class DynamoTableIndex
    {
        public string PartitionKey { get; }
        public string SortKey { get; }
        public int SearchPriority { get; }
        public string Index { get; }
        public DynamoTableIndex(string partitionKey, string sortKey, int searchPriority = 0)
        {
            if (partitionKey.Length <= 1 || partitionKey.Length > 255)
            {
                throw new ArgumentException("PartitionKey must be longer then 1 and smaller then 255");
            }

            if (sortKey.Length <= 1 || sortKey.Length > 255)
            {
                throw new ArgumentException("SortKey must be longer then 1 and smaller then 255");
            }

            PartitionKey = partitionKey;
            SortKey = sortKey;
            SearchPriority = searchPriority;
            Index = $"{partitionKey}_{sortKey}_index";
        }
    }
}
