using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Docker.DotNet.Models;
using DockerHelperLib;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace DynamoDB.Common.FunctionalTests
{
    [SetUpFixture]
    public class TestsSetUp
    {
        private const string AWSRegion = "AWS:RegionName";
        private const string AWSAccessKey = "AWS:AccessKey";
        private const string AWSSecretKey = "AWS:SecretKey";
        private const string AWSDynamoDbUrl = "AWS:DynamoDB:ServiceUrl";

        private DockerServer _dockerServer;
        private ContainerListResponse _dynamoContainer;

        internal static IAmazonDynamoDB Client;
        internal static IDynamoDBContext Context;

        private static IConfiguration configuration = null;

        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            if (Configuration.GetValue<bool>("StartDynamoDocker"))
            {
                _dockerServer = new DockerServer();
                _dynamoContainer = _dockerServer.RunContainter().Result;
            }

            Client = new AmazonDynamoDBClient(AccessKey, SecretKey, CreateAmazonDynamoDBConfig());
            Context = new DynamoDBContext(Client);
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            if (_dynamoContainer != null)
            {
                _dockerServer?.StopContainer(_dynamoContainer).Wait();
            }
        }

        private static IConfiguration Configuration => configuration ?? (configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build());

        private static string ServiceURL
        {
            get
            {
                return Configuration.GetSection(AWSDynamoDbUrl).Value;
            }
        }

        private static RegionEndpoint RegionEndPoint
        {
            get
            {
                return RegionEndpoint.GetBySystemName(Configuration.GetSection(AWSRegion).Value);
            }
        }

        private static string AccessKey
        {
            get
            {
                return Configuration.GetSection(AWSAccessKey).Value;
            }
        }

        private static string SecretKey
        {
            get
            {
                return Configuration.GetSection(AWSSecretKey).Value;
            }
        }

        private static AmazonDynamoDBConfig CreateAmazonDynamoDBConfig()
        {
            var clientConfig = new AmazonDynamoDBConfig();
            clientConfig.RegionEndpoint = RegionEndPoint;
            if (ServiceURL != null)
            {
                clientConfig.ServiceURL = ServiceURL;
            }

            return clientConfig;
        }
    }
}
