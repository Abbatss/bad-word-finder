using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BWF.Api.Host.Controllers;
using BWF.Api.Services.Models;
using BWF.Api.Services.Store;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BWF.Api.Host.UnitTests
{
    [TestFixture]
    public class BadWordsControllerTests
    {
        private readonly Mock<IBadWordsRepository> mock = new Mock<IBadWordsRepository>();

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Constructor_Test()
        {
            Assert.Throws<ArgumentNullException>(() => new BadWordsController(null));
        }

        [Test]
        public async Task Get_Success_Test()
        {
            string id = "id";

            mock.Invocations.Clear();
            mock.Setup(p => p.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new BadWord());
            var controller = new BadWordsController(mock.Object);

            var res = await controller.Get(id);
            Assert.IsInstanceOf<BadWord>(res);
        }

    }
}
