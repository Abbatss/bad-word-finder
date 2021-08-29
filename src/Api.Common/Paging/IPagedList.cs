namespace Api.Common.Paging
{
    public interface IPagedList<T>
    {
    }

    public interface IPagedList
    {
        PagingParams PrevPageParam { get; set; }

        PagingParams NextPageParam { get; set; }

        PagingParams CurrentPageParam { get; set; }
    }
}
