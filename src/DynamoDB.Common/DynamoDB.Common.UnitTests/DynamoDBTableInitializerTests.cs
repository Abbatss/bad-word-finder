using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDB.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace DynamoDB.Common.UnitTests
{
    public class DynamoDDBInitializerTests
    {
        private readonly Mock<IAmazonDynamoDB> _dynamoDB = new Mock<IAmazonDynamoDB>();
        private readonly Mock<IDynamoDBTableDefinition> _dynamoTableDefinition = new Mock<IDynamoDBTableDefinition>();
        private readonly Mock<ILogger<DynamoDBTableInitializer>> _logger = new Mock<ILogger<DynamoDBTableInitializer>>();
        [OneTimeSetUp]
        public void Setup()
        {
            // _logger.Setup(p => p.LogInformation(It.IsAny<string>(), new object[] { }));
        }

        [Test]
        public void Constructor1Test()
        {
            Assert.DoesNotThrow(() => new DynamoDBTableInitializer(_dynamoDB.Object, "abc", _logger.Object, 1, _dynamoTableDefinition.Object));
            Assert.Throws<ArgumentNullException>(() => new DynamoDBTableInitializer(_dynamoDB.Object, "    ", _logger.Object, 1, _dynamoTableDefinition.Object), "tableName");
            Assert.Throws<ArgumentNullException>(() => new DynamoDBTableInitializer(null, "abs", _logger.Object, 1, _dynamoTableDefinition.Object), "client");
            Assert.Throws<ArgumentNullException>(() => new DynamoDBTableInitializer(_dynamoDB.Object, "abc", null, 1, _dynamoTableDefinition.Object), "logger");
            Assert.Throws<ArgumentNullException>(() => new DynamoDBTableInitializer(_dynamoDB.Object, "abc", _logger.Object, 1, null), "tableDefinition");
        }

        [Test]
        public void Constructor2Test()
        {
            Assert.DoesNotThrow(() => new DynamoDBTableInitializer(_dynamoDB.Object, "abc", _logger.Object, _dynamoTableDefinition.Object));
            Assert.Throws<ArgumentNullException>(() => new DynamoDBTableInitializer(_dynamoDB.Object, "    ", _logger.Object, _dynamoTableDefinition.Object), "tableName");
            Assert.Throws<ArgumentNullException>(() => new DynamoDBTableInitializer(null, "abs", _logger.Object, _dynamoTableDefinition.Object), "client");
            Assert.Throws<ArgumentNullException>(() => new DynamoDBTableInitializer(_dynamoDB.Object, "abc", null, _dynamoTableDefinition.Object), "logger");
            Assert.Throws<ArgumentNullException>(() => new DynamoDBTableInitializer(_dynamoDB.Object, "abc", _logger.Object, null), "tableDefinition");
        }

        [Test]
        public async Task IsTableExistTrueTest()
        {
            var response = new DescribeTableResponse();
            response.Table = new TableDescription();
            response.Table.TableStatus = "Active";
            _dynamoDB.Setup(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var initializer = new DynamoDBTableInitializer(_dynamoDB.Object, "Table1", _logger.Object, 2, _dynamoTableDefinition.Object);
            Assert.IsTrue(await initializer.IsTableExists().ConfigureAwait(false));
        }

        [Test]
        public async Task IsTableExistFalseTest()
        {
            _dynamoDB.Reset();
            _dynamoDB.Setup(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ResourceNotFoundException("Table"));
            var initializer = new DynamoDBTableInitializer(_dynamoDB.Object, "Table1", _logger.Object, 2, _dynamoTableDefinition.Object);
            Assert.IsFalse(await initializer.IsTableExists().ConfigureAwait(false));
        }

        [Test]
        public async Task IsTableExistDataBaseInitializationExceptionTest()
        {
            var response = new DescribeTableResponse();
            response.Table = new TableDescription();
            response.Table.TableStatus = "NotActive";
            _dynamoDB.Reset();
            _dynamoDB.Setup(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            _dynamoTableDefinition.Reset();
            var initializer = new DynamoDBTableInitializer(_dynamoDB.Object, "Table1", _logger.Object, 2, _dynamoTableDefinition.Object);
            Assert.ThrowsAsync<DataBaseInitializationException>(() => initializer.IsTableExists());
            _dynamoDB.Verify(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Test]
        public async Task CreateTableSuccessTest()
        {
            _dynamoTableDefinition.Reset();
            var response = new DescribeTableResponse();
            response.Table = new TableDescription();
            response.Table.TableStatus = "Active";
            response.Table.TableName = "Table1";
            _dynamoDB.Reset();
            _dynamoDB.Setup(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var createRequest = new CreateTableRequest();
            var createResponse = new CreateTableResponse();
            createRequest.TableName = response.Table.TableName;
            _dynamoDB.Setup(p => p.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createResponse); ;
            var initializer = new DynamoDBTableInitializer(_dynamoDB.Object, response.Table.TableName, _logger.Object, 2, _dynamoTableDefinition.Object);
            await initializer.CreateTable().ConfigureAwait(false);
            _dynamoDB.Verify(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            _dynamoDB.Verify(p => p.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
            _dynamoTableDefinition.Verify(p => p.AddIndexes(It.IsAny<CreateTableRequest>()), Times.Once);
        }

        [Test]
        public async Task CreateTableFailTest()
        {
            var response = new DescribeTableResponse();
            response.Table = new TableDescription();
            response.Table.TableStatus = "NotActive";
            response.Table.TableName = "Table1";
            _dynamoDB.Reset();
            _dynamoDB.SetupSequence(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ResourceNotFoundException("Table 1")).ReturnsAsync(response);
            var createRequest = new CreateTableRequest();
            var createResponse = new CreateTableResponse();
            createRequest.TableName = response.Table.TableName;
            _dynamoDB.Setup(p => p.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createResponse); ;
            var initializer = new DynamoDBTableInitializer(_dynamoDB.Object, response.Table.TableName, _logger.Object, 2, _dynamoTableDefinition.Object);
            Assert.ThrowsAsync<DataBaseInitializationException>(() => initializer.CreateTable());
            _dynamoDB.Verify(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _dynamoDB.Verify(p => p.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        [Test]
        public async Task InitTableNotExistsTest()
        {
            var response = new DescribeTableResponse();
            response.Table = new TableDescription();
            response.Table.TableStatus = "Active";
            response.Table.TableName = "Table1";
            _dynamoDB.Reset();
            _dynamoDB.SetupSequence(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new ResourceNotFoundException("Table 1")).ReturnsAsync(response);
            var createRequest = new CreateTableRequest();
            var createResponse = new CreateTableResponse();
            createRequest.TableName = response.Table.TableName;
            _dynamoDB.Setup(p => p.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createResponse); ;
            var initializer = new DynamoDBTableInitializer(_dynamoDB.Object, response.Table.TableName, _logger.Object, 2, _dynamoTableDefinition.Object);
            Assert.DoesNotThrowAsync(() => initializer.Init());
            _dynamoDB.Verify(p => p.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        }

        [Test]
        public async Task InitTableExistsTest()
        {
            var response = new DescribeTableResponse();
            response.Table = new TableDescription();
            response.Table.TableStatus = "Active";
            response.Table.TableName = "Table1";
            _dynamoDB.Reset();
            _dynamoDB.SetupSequence(p => p.DescribeTableAsync(It.IsAny<DescribeTableRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            var createRequest = new CreateTableRequest();
            var createResponse = new CreateTableResponse();
            createRequest.TableName = response.Table.TableName;
            _dynamoDB.Setup(p => p.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(createResponse); ;
            var initializer = new DynamoDBTableInitializer(_dynamoDB.Object, response.Table.TableName, _logger.Object, 2, _dynamoTableDefinition.Object);
            Assert.DoesNotThrowAsync(() => initializer.Init());
            _dynamoDB.Verify(p => p.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()), Times.Exactly(0));
        }
    }
}
