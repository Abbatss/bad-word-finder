using System;
using System.Threading;
using System.Threading.Tasks;
using BWF.Api.Services.Models;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;

namespace BWF.Api.Services.DynamoDB
{
    // TODO: consider limitations 400kb for AllWordsRecord. We may need to split to smalle chanks based on first letter as a key?
    public class WordsRepository : Store.IBadWordsRepository
    {
        private IDynamoDBContext Context { get; }
        private string TableName { get; }
        private DynamoDBOperationConfig OperationConfig { get; }
        public WordsRepository(IDynamoDBContext context, string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            TableName = tableName;
            Context = context ?? throw new ArgumentNullException(nameof(context));
            this.OperationConfig = new DynamoDBOperationConfig()
            {
                SkipVersionCheck = true,
                OverrideTableName = TableName,
                ConsistentRead = true,

            };
        }

        private const string defaultAggregateId = "a";

        // TODO: add transaction
        public async Task<BadWord> AddAsync(BadWord word, CancellationToken cancellationToken = default)
        {
            var record = new WordRecord(word);
            await Context.SaveAsync(record, OperationConfig, cancellationToken);
            var aggregateRecord = await GetAllWorldsRecord(cancellationToken);
            aggregateRecord.Data.Add(word);
            await Context.SaveAsync(aggregateRecord, OperationConfig, cancellationToken);
            return word;
        }

        private async Task<AllWordsRecord> GetAllWorldsRecord(CancellationToken cancellationToken = default)
        {
            var aggregateRecord = await Context.LoadAsync(new AllWordsRecord(defaultAggregateId), OperationConfig, cancellationToken);
            if (aggregateRecord == null)
            {
                aggregateRecord = new AllWordsRecord(defaultAggregateId);
            }

            return aggregateRecord;
        }

        public async Task<BadWord> GetAsync(string wordId, CancellationToken cancellationToken = default)
        {
            var record = await Context.LoadAsync(new WordRecord(wordId), OperationConfig, cancellationToken);
            return record?.Data;
        }

        public async Task DeleteAsync(string wordId, CancellationToken cancellationToken = default)
        {
            var aggregateRecord = await GetAllWorldsRecord(cancellationToken);

            var elem = aggregateRecord.Data.Find(c => c.Id == wordId);
            if (elem == null)
            {
                return;
            }
            aggregateRecord.Data.Remove(elem);

            // TODO: add transaction
            await Context.SaveAsync(aggregateRecord, OperationConfig, cancellationToken);
            await Context.DeleteAsync(new WordRecord(wordId), OperationConfig, cancellationToken);
        }

        public async Task<List<BadWord>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var aggregateRecord = await Context.LoadAsync(new AllWordsRecord(defaultAggregateId), OperationConfig, cancellationToken);
            if (aggregateRecord == null)
            {
                aggregateRecord = new AllWordsRecord(defaultAggregateId);
            }
            return aggregateRecord.Data;
        }
    }
}
