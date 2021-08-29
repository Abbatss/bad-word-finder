using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Api.Common.Paging
{
    public class PagedList<T> : IPagedList<T>, IPagedList, IEnumerable<T>
    {
        public List<T> Items { get; set; }
        [JsonIgnore]
        public PagingParams PrevPageParam { get; set; }
        [JsonIgnore]
        public PagingParams NextPageParam { get; set; }
        [JsonIgnore]
        public PagingParams CurrentPageParam { get; set; }

        public PagedList()
        {
        }

        public PagedList(IEnumerable<T> items, PagingParams prevPageParam, PagingParams currentPageParam, PagingParams nextPageParam)
        {
            Items = items.ToList();
            PrevPageParam = prevPageParam;
            NextPageParam = nextPageParam;
            CurrentPageParam = currentPageParam;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
