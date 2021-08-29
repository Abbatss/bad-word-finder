using NUnit.Framework;
using DynamoDB.Common.Extensions;
namespace DynamoDB.Common.UnitTests
{
    public class PaginationTokenExtensionTests
    {
        [Test]
        public void PaginationTokenExtensionTest()
        {
            var token = "{\"Id\"{\"S\":\"db86d4eb-f2c0-4383-b760-5df78f2583d8\"}}";
            Assert.IsFalse(token.IsEndOrBegin());
        }

        [TestCase(null)]
        [TestCase("{}")]
        [TestCase("")]
        public void PaginationTokenExtensionWrongTest(string token)
        {
            Assert.IsTrue(token.IsEndOrBegin());
        }
    }
}
