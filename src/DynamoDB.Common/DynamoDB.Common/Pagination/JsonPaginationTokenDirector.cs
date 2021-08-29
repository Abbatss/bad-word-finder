using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;

namespace DynamoDB.Common.Pagination
{
    public interface IJsonPaginationTokenDirector
    {
        JsonPaginationTokenBuilder Make();
    }

    public class JsonPaginationTokenFilterDirector : IJsonPaginationTokenDirector
    {
        private readonly Dictionary<string, string> tokenKeys = new Dictionary<string, string>();

        public JsonPaginationTokenFilterDirector(DynamoTableIndex tableIndex, IDictionary<string, DynamoDBEntry> document)
        {
            tokenKeys[tableIndex.PartitionKey] = document.Single(c => c.Key == tableIndex.PartitionKey).Value.AsString();
            tokenKeys[tableIndex.SortKey] = document.Single(c => c.Key == tableIndex.SortKey).Value.AsString();
            tokenKeys[DynamoTableConstants.PrimaryPartitionKey] = document.Single(c => c.Key == DynamoTableConstants.PrimaryPartitionKey).Value.AsString();
            tokenKeys[DynamoTableConstants.PrimarySortKey] = document.Single(c => c.Key == DynamoTableConstants.PrimarySortKey).Value.AsString();
        }

        public JsonPaginationTokenBuilder Make()
        {
            var builder = new JsonPaginationTokenBuilder();
            builder.Add(tokenKeys);
            return builder;
        }
    }
}