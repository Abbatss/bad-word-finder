namespace DynamoDB.Common
{
    using System.Collections.Generic;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using DynamoDB.Common.Extensions;

    public class TableWithSortDataDefinition : IDynamoDBTableDefinition
    {
        public DynamoTableIndex DefaultIndex => DynamoTableConstants.SortDataIndex;

        public void AddIndexes(CreateTableRequest createTableRequest)
        {
            var projection = new Projection()
            {
                ProjectionType = ProjectionType.ALL,
            };
            createTableRequest.AddIndex(DynamoTableConstants.SortDataIndex, projection);
        }

        public IEnumerable<DynamoTableIndex> GetIndexes()
        {
            yield return DynamoTableConstants.SortDataIndex;
        }
    }
}
