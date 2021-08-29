using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using DynamoDB.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DynamoDB.Common.FunctionalTests
{
    public class DynamoDBTableInitializerFunctionalTests
    {
        private readonly Mock<ILogger<DynamoDBTableInitializer>> _logger = new Mock<ILogger<DynamoDBTableInitializer>>();
        private string currentTableName = string.Empty;
        [OneTimeSetUp]
        public async Task Setup()
        {
        }

        [TearDown]
        public async Task ClearTable()
        {
            if (!string.IsNullOrWhiteSpace(currentTableName))
            {
                try
                {
                    await TestsSetUp.Client.DeleteTableAsync(currentTableName).ConfigureAwait(false);
                }
                catch
                { }
            }
        }

        [Test]
        public async Task CreateTest()
        {
            currentTableName = "CreateTestTable";
            var initializer = new DynamoDBTableInitializer(TestsSetUp.Client, currentTableName, _logger.Object, new TableWithSortDataDefinition());
            await initializer.CreateTable().ConfigureAwait(false);
            Assert.IsTrue(await initializer.IsTableExists().ConfigureAwait(false));
        }

        [Test]
        public async Task IsTableExistTest()
        {
            currentTableName = "IsTableExistTestTable";
            var initializer = new DynamoDBTableInitializer(TestsSetUp.Client, currentTableName, _logger.Object, new TableWithSortDataDefinition());
            Assert.IsFalse(await initializer.IsTableExists().ConfigureAwait(false));
            await initializer.CreateTable().ConfigureAwait(false);
            Assert.IsTrue(await initializer.IsTableExists().ConfigureAwait(false));
        }

        [Test]
        public async Task InitTest()
        {
            currentTableName = "InitTestTable";
            var initializer = new DynamoDBTableInitializer(TestsSetUp.Client, currentTableName, _logger.Object, new TableWithSortDataDefinition());
            await initializer.Init().ConfigureAwait(false);
            Assert.IsTrue(await initializer.IsTableExists().ConfigureAwait(false));
            await TestsSetUp.Client.DeleteTableAsync(currentTableName).ConfigureAwait(false);
        }

        [Test]
        public async Task InitFailTest()
        {
            var clientConfig = new AmazonDynamoDBConfig();
            clientConfig.RegionEndpoint = Amazon.RegionEndpoint.EUWest2;
            clientConfig.ServiceURL = "http://localhost2:8000";
            var client = new AmazonDynamoDBClient("Test", "Test", clientConfig);
            var initializer = new DynamoDBTableInitializer(client, "InitFailTest", _logger.Object, 1, new TableWithSortDataDefinition());
            Assert.ThrowsAsync<DataBaseInitializationException>(() => initializer.Init());
        }
    }
}
