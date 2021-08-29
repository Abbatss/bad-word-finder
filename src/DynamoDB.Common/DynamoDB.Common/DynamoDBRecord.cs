using Amazon.DynamoDBv2.DataModel;

namespace DynamoDB.Common
{
    public class DynamoDBRecord
    {
        [DynamoDBHashKey(DynamoTableConstants.PrimaryPartitionKey)]
        public string Id { get; set; }
        [DynamoDBProperty(DynamoTableConstants.PrimarySortKey)]
        public string SK { get; set; }
        [DynamoDBProperty(DynamoTableConstants.SortData_SPK)]
        public string SortData { get; set; }
        [DynamoDBVersion]
        public int? VersionNumber { get; set; }
        public DynamoDBRecord()
        {
        }

        public DynamoDBRecord(string id, string sk, string sortData, int? versionNumber)
        {
            Id = id;
            SK = sk;
            SortData = sortData;
            VersionNumber = versionNumber;
        }

        public string GetKey()
        {
            return $"{Id}#{SK}";
        }
    }
}