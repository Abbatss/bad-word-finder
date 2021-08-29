using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamoDB.Common.Pagination
{
    public interface ICursorPagination<T>
    {
        string FirstPaginationToken { get; }
        string LastPaginationToken { get; }
        IEnumerable<T> Items { get; }
        Task Query(int limit, string paginationToken, bool backwardSearch = false);
    }
}
