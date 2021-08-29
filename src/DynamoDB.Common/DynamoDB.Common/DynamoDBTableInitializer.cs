using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDB.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;

namespace DynamoDB.Common
{
    /// <summary>
    /// class to initialize aws dymano DB.
    /// </summary>
    public class DynamoDBTableInitializer : IEntityDBInitializer
    {
        private const int DBInitializeDefaultRetryCount = 100;
        private const string ActiveTableStatus = "ACTIVE";
        private readonly ILogger<DynamoDBTableInitializer> logger;
        private IAmazonDynamoDB Client { get; }
        private string TableName { get; }
        private readonly int retryPolicy;
        private readonly IDynamoDBTableDefinition tableDefinition;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">Dynamo DB client.</param>
        /// <param name="tableName">Dynamo DB table name to create. </param>
        /// <param name="logger"> logger. </param>
        /// <param name="retryCount"> how long and during which intervals try to check Dynamo DB.</param>
        /// <param name="tableDefinition">definition how to create dynamo table and which indexes should be added.</param>
        public DynamoDBTableInitializer(IAmazonDynamoDB client, string tableName, ILogger<DynamoDBTableInitializer> logger, int retryCount, IDynamoDBTableDefinition tableDefinition)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentNullException(nameof(tableName));
            }

            Client = client ?? throw new ArgumentNullException(nameof(client));
            TableName = tableName;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.retryPolicy = retryCount;
            this.tableDefinition = tableDefinition ?? throw new ArgumentNullException(nameof(tableDefinition));
        }

        /// <summary>
        /// default constructor. will initialize default retry policy.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="tableName"></param>
        /// <param name="logger"></param>
        /// <param name="tableDefinition"></param>
        public DynamoDBTableInitializer(IAmazonDynamoDB client, string tableName, ILogger<DynamoDBTableInitializer> logger, IDynamoDBTableDefinition tableDefinition) : this(client, tableName, logger, DBInitializeDefaultRetryCount, tableDefinition)
        {
        }

        /// <summary>
        /// method to initialize dynamoDB.
        /// </summary>
        /// <returns>void.</returns>
        public async Task Init()
        {
            logger.LogInformation("Initializing of Dynamo DB started");
            if (!await IsTableExists().ConfigureAwait(false))
            {
                await CreateTable().ConfigureAwait(false);
            }
            else
            {
                logger.LogInformation("Table already exists");
            }

            logger.LogInformation("Initializing of Dynamo DB finished");
        }

        /// <summary>
        /// Check if Dynamo Table exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> IsTableExists()
        {
            var res = await Policy.HandleResult<DescribeTableResponse>(r => r?.Table?.TableStatus != ActiveTableStatus)
                .Or<HttpRequestException>()
                            .WaitAndRetryAsync(retryPolicy, (c) => TimeSpan.FromSeconds(2)).ExecuteAndCaptureAsync(() => Client.DescribeTableAsync(new DescribeTableRequest
                            {
                                TableName = TableName,
                            })).ConfigureAwait(false);
            if (res.Outcome == OutcomeType.Successful)
            {
                return true;
            }

            if (res.FinalException is null)
            {
                throw new DataBaseInitializationException("Dynamo Table exists but not in Active state");
            }

            if (res.FinalException is ResourceNotFoundException)
            {
                return false;
            }

            throw new DataBaseInitializationException("Something goes wrong when inititalize DB. Please check inner exception", res.FinalException);
        }

        /// <summary>
        /// create new table in Dynamo DB.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateTable()
        {
            logger.LogInformation($"Creation of table {TableName} started");
            var createRequest = new CreateTableRequest();
            createRequest.TableName = TableName;
            createRequest.BillingMode = BillingMode.PAY_PER_REQUEST;
            createRequest.KeySchema = new List<KeySchemaElement>()
                    {
                new KeySchemaElement(DynamoTableConstants.PrimaryPartitionKey, KeyType.HASH),
                new KeySchemaElement(DynamoTableConstants.PrimarySortKey, KeyType.RANGE),
                    };
            createRequest.AttributeDefinitions.Add(new AttributeDefinition(DynamoTableConstants.PrimaryPartitionKey, ScalarAttributeType.S));
            createRequest.AttributeDefinitions.Add(new AttributeDefinition(DynamoTableConstants.PrimarySortKey, ScalarAttributeType.S));
            tableDefinition.AddIndexes(createRequest);
            try
            {
                var respose = await Client.CreateTableAsync(createRequest).ConfigureAwait(false);
            }
            catch (ResourceInUseException)
            {
            }

            var retry = Policy.Handle<ResourceNotFoundException>()
                .Or<ResourceInUseException>()
                .OrResult<DescribeTableResponse>(r => r?.Table?.TableStatus != ActiveTableStatus)
                .WaitAndRetryAsync(retryPolicy, (c) => TimeSpan.FromSeconds(2));

            var res = await retry.ExecuteAndCaptureAsync(() => Client.DescribeTableAsync(new DescribeTableRequest
            {
                TableName = TableName,
            })).ConfigureAwait(false);
            if (res.Outcome == OutcomeType.Failure)
            {
                throw new DataBaseInitializationException("Dynamo DB was not initialized.", res.FinalException);
            }

            logger.LogInformation($"Creation of table {TableName} finished");
        }
    }
}
