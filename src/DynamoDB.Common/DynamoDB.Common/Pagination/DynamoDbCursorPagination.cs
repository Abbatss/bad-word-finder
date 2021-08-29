using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using DynamoDB.Common.Extensions;

namespace DynamoDB.Common.Pagination
{
    public class DynamoDbCursorPagination<T> : ICursorPagination<T>
    {
        private readonly Table table;
        private readonly IDynamoDBContext сontext;
        private Search search;
        private DynamoDbFiltering dynamoDbFiltering;

        public string FirstPaginationToken { get; private set; }
        public string LastPaginationToken { get; private set; }
        public IEnumerable<T> Items { get; private set; }

        public DynamoDbCursorPagination(Table table, IDynamoDBContext сontext, IDynamoDBTableDefinition tableDefinition)
        {
            if (tableDefinition is null)
            {
                throw new ArgumentNullException(nameof(tableDefinition));
            }

            this.table = table ?? throw new ArgumentNullException(nameof(table));
            this.сontext = сontext ?? throw new ArgumentNullException(nameof(сontext));
            dynamoDbFiltering = new DynamoDbFiltering(tableDefinition.GetIndexes(), tableDefinition.DefaultIndex);
        }

        public void AddFilter(string attributeName, ScanOperator scanOperator, string value)
        {
            dynamoDbFiltering.AddFilter(attributeName, scanOperator, value);
        }

        public async Task Query(int limit, string paginationToken, bool backwardSearch = false)
        {
            var tableIndex = dynamoDbFiltering.ChooseIndex();
            var config = CreateConfigOperation(tableIndex, paginationToken, backwardSearch);
            dynamoDbFiltering.AddFilters(tableIndex, config);
            IEnumerable<Document> answer = await GetQuery(limit, config).ConfigureAwait(false);
            SetPaginationTokens(tableIndex, search, answer, backwardSearch);
        }

        private QueryOperationConfig CreateConfigOperation(DynamoTableIndex tableIndex, string paginationToken, bool backwardSearch)
        {
            return new QueryOperationConfig()
            {
                PaginationToken = paginationToken,
                BackwardSearch = backwardSearch,
                IndexName = tableIndex.Index,
                Select = SelectValues.AllProjectedAttributes,
                Filter = new QueryFilter(),
            };
        }

        private async Task<IEnumerable<Document>> GetQuery(int limit, QueryOperationConfig config)
        {
            List<Document> answer = new List<Document>();
            do
            {
                config.Limit = limit - answer.Count;
                search = table.Query(config);
                answer.AddRange(await search.GetNextSetAsync().ConfigureAwait(false));
                config.PaginationToken = search.PaginationToken;
            } while (answer.Count < limit && !config.PaginationToken.IsEndOrBegin());
            return answer;
        }

        private void SetPaginationTokens(DynamoTableIndex tableIndex, Search search, IEnumerable<Document> items, bool backwardSearch)
        {
            var firstDocument = items.FirstOrDefault();

            var firstToken = firstDocument != null ? new JsonPaginationTokenFilterDirector(tableIndex, firstDocument).Make().Build() : null;
            var lastToken = search.PaginationToken;

            if (backwardSearch)
            {
                items = items.Reverse();
                FirstPaginationToken = lastToken;
                LastPaginationToken = firstToken;
            }
            else
            {
                FirstPaginationToken = firstToken;
                LastPaginationToken = lastToken;
            }

            Items = сontext.FromDocuments<T>(items);
        }
    }
}