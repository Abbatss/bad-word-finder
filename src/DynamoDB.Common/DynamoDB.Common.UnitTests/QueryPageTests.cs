using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Api.Common.Paging;
using DynamoDB.Common.Pagination;
using Moq;
using NUnit.Framework;

namespace DynamoDB.Common.UnitTests
{
    public class QueryPageTests
    {
        [Test]
        public async Task QueryPage_Query_Test()
        {
            var mockCursorPagination = new Mock<ICursorPagination<object>>();
            var mockDynamoDbContext = new Mock<IDynamoDBContext>();
            mockCursorPagination.Setup(c => c.Items).Returns(new List<object>());
            mockCursorPagination.Setup(c => c.FirstPaginationToken).Returns("first");
            mockCursorPagination.Setup(c => c.LastPaginationToken).Returns("last");
            var pagingParams = new PagingParams("current", ScanDirections.Forward, 100);

            QueryPage<object> queryPage = new QueryPage<object>(mockCursorPagination.Object);
            await queryPage.Query(pagingParams).ConfigureAwait(false);
            Assert.IsNotNull(queryPage.PreviousPagingParam);
            Assert.IsNotNull(queryPage.NextPagingParam);
        }

        [Test]
        public async Task QueryPage_QueryFirstPage_Test()
        {
            var mockCursorPagination = new Mock<ICursorPagination<object>>();
            var mockDynamoDbContext = new Mock<IDynamoDBContext>();
            mockCursorPagination.Setup(c => c.Items).Returns(new List<object> ());
            mockCursorPagination.Setup(c => c.FirstPaginationToken).Returns("first");
            mockCursorPagination.Setup(c => c.LastPaginationToken).Returns("last");
            var pagingParams = new PagingParams("", ScanDirections.Forward, 100);

            QueryPage<object> queryPage = new QueryPage<object>(mockCursorPagination.Object);
            await queryPage.Query(pagingParams).ConfigureAwait(false);
            Assert.IsNull(queryPage.PreviousPagingParam);
            Assert.IsNotNull(queryPage.NextPagingParam);
        }

        [Test]
        public async Task QueryPage_QueryLastPage_Test()
        {
            var mockCursorPagination = new Mock<ICursorPagination<object>>();
            var mockDynamoDbContext = new Mock<IDynamoDBContext>();
            mockCursorPagination.Setup(c => c.Items).Returns(new List<object>());
            mockCursorPagination.Setup(c => c.FirstPaginationToken).Returns("first");
            mockCursorPagination.Setup(c => c.LastPaginationToken).Returns("last");
            var pagingParams = new PagingParams("", ScanDirections.Backward, 100);

            QueryPage<object> queryPage = new QueryPage<object>(mockCursorPagination.Object);
            await queryPage.Query(pagingParams).ConfigureAwait(false);
            Assert.IsNotNull(queryPage.PreviousPagingParam);
            Assert.IsNull(queryPage.NextPagingParam);
        }

        [Test]
        public async Task QueryPage_QueryToLastPage_Test()
        {
            var mockCursorPagination = new Mock<ICursorPagination<object>>();
            var mockDynamoDbContext = new Mock<IDynamoDBContext>();
            mockCursorPagination.Setup(c => c.Items).Returns(new List<object>());
            mockCursorPagination.Setup(c => c.FirstPaginationToken).Returns("first");
            mockCursorPagination.Setup(c => c.LastPaginationToken).Returns("");
            var pagingParams = new PagingParams("current", ScanDirections.Forward, 100);

            QueryPage<object> queryPage = new QueryPage<object>(mockCursorPagination.Object);
            await queryPage.Query(pagingParams).ConfigureAwait(false);
            Assert.IsNotNull(queryPage.PreviousPagingParam);
            Assert.IsNull(queryPage.NextPagingParam);
        }

        [Test]
        public async Task QueryPage_QueryToFirstPage_Test()
        {
            var mockCursorPagination = new Mock<ICursorPagination<object>>();
            var mockDynamoDbContext = new Mock<IDynamoDBContext>();
            mockCursorPagination.Setup(c => c.Items).Returns(new List<object>());
            mockCursorPagination.Setup(c => c.FirstPaginationToken).Returns("");
            mockCursorPagination.Setup(c => c.LastPaginationToken).Returns("last");
            var pagingParams = new PagingParams("current", ScanDirections.Backward, 100);

            QueryPage<object> queryPage = new QueryPage<object>(mockCursorPagination.Object);
            await queryPage.Query(pagingParams).ConfigureAwait(false);
            Assert.IsNull(queryPage.PreviousPagingParam);
            Assert.IsNotNull(queryPage.NextPagingParam);
        }

        [Test]
        public async Task QueryPage_QueryAfterLastPage_Test()
        {
            var mockCursorPagination = new Mock<ICursorPagination<object>>();
            var mockDynamoDbContext = new Mock<IDynamoDBContext>();
            mockCursorPagination.Setup(c => c.Items).Returns(new List<object>());
            mockCursorPagination.Setup(c => c.FirstPaginationToken).Returns("");
            mockCursorPagination.Setup(c => c.LastPaginationToken).Returns("");
            var pagingParams = new PagingParams("current", ScanDirections.Forward, 100);

            QueryPage<object> queryPage = new QueryPage<object>(mockCursorPagination.Object);
            await queryPage.Query(pagingParams).ConfigureAwait(false);
            Assert.IsNotNull(queryPage.PreviousPagingParam);
            Assert.IsNull(queryPage.NextPagingParam);
        }

        [Test]
        public async Task QueryPage_QueryBeforeFirstPage_Test()
        {
            var mockCursorPagination = new Mock<ICursorPagination<object>>();
            var mockDynamoDbContext = new Mock<IDynamoDBContext>();
            mockCursorPagination.Setup(c => c.Items).Returns(new List<object>());
            mockCursorPagination.Setup(c => c.FirstPaginationToken).Returns("");
            mockCursorPagination.Setup(c => c.LastPaginationToken).Returns("");
            var pagingParams = new PagingParams("current", ScanDirections.Backward, 100);

            QueryPage<object> queryPage = new QueryPage<object>(mockCursorPagination.Object);
            await queryPage.Query(pagingParams).ConfigureAwait(false);
            Assert.IsNull(queryPage.PreviousPagingParam);
            Assert.IsNotNull(queryPage.NextPagingParam);
        }
    }
}
