using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Common.Paging;
using DynamoDB.Common.Extensions;

namespace DynamoDB.Common.Pagination
{
    public class QueryPage<T>
    {
        private ICursorPagination<T> cursorPagination;

        public PagingParams PreviousPagingParam { get; private set; }
        public PagingParams NextPagingParam { get; private set; }

        public IEnumerable<T> Items
        {
            get
            {
                return cursorPagination.Items;
            }
        }

        public QueryPage(ICursorPagination<T> cursorPagination)
        {
            this.cursorPagination = cursorPagination;
        }

        public async Task Query(PagingParams pagingParams)
        {
            await cursorPagination.Query(pagingParams.Limit, pagingParams.Cursor, pagingParams.ScanDirection == ScanDirections.Backward).ConfigureAwait(false);

            PreviousPagingParam = (pagingParams.Cursor.IsEndOrBegin() && pagingParams.ScanDirection == ScanDirections.Forward) ? null : GetPreviousPage(cursorPagination.FirstPaginationToken, pagingParams.ScanDirection, pagingParams.Limit);
            NextPagingParam = (pagingParams.Cursor.IsEndOrBegin() && pagingParams.ScanDirection == ScanDirections.Backward) ? null : GetNextPage(cursorPagination.LastPaginationToken, pagingParams.ScanDirection, pagingParams.Limit);
        }

        private PagingParams GetPreviousPage(string firstToken, ScanDirections scanDirection, int limit)
        {
            return (firstToken.IsEndOrBegin() && scanDirection == ScanDirections.Backward) ? null : new PagingParams(firstToken, ScanDirections.Backward, limit);
        }

        private PagingParams GetNextPage(string lastToken, ScanDirections scanDirection, int limit)
        {
            return (lastToken.IsEndOrBegin() && scanDirection == ScanDirections.Forward) ? null : new PagingParams(lastToken, ScanDirections.Forward, limit);
        }
    }
}
