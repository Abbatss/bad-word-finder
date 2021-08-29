using BWF.Api.Services.Store;
using NUnit.Framework;
using Moq;
using BWF.Api.Services.Models;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BWF.Api.Services.UnitTests
{
    public class Tests
    {
        private readonly Mock<IBadWordsRepository> mock = new Mock<IBadWordsRepository>();

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
            mock.Invocations.Clear();
        }

        [TestCase("abc", 1)]
        [TestCase("ab c", 0)]
        [TestCase("abc abc ", 2)]
        [TestCase("ab abc ab c", 1)]
        [TestCase(" abc     abc", 2)]
        [TestCase("ABC", 1)]
        [TestCase("Abc", 1)]
        [TestCase("", 0)]
        public async Task RunTest(string text, int expectedCount)
        {
            mock.Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<BadWord>() { new BadWord { Word = "abc" }, new BadWord { Word = "XXXXXX" } });
            var engine = new CheckEngine(mock.Object);
            byte[] byteArray = Encoding.ASCII.GetBytes(text);
            using (var stream = new MemoryStream(byteArray))
            {
                var res = await engine.Run(stream);
                Assert.NotNull(res);
                Assert.Equals(1, res.Words.Count);
                Assert.Equals(expectedCount, res.Words["abc"]);
            }

        }
    }
}
