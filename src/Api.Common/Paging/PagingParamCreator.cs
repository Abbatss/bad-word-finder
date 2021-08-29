
namespace Api.Common.Paging
{
    public static class PagingParamCreator
    {
        public static PagingParams Create(string start, bool back, int? num)
        {
            var paginationToken = start?.FromBase64String();
            var scanDirection = back ? ScanDirections.Backward : ScanDirections.Forward;
            if (num == null)
            {
                return new PagingParams(paginationToken, scanDirection);
            }

            return new PagingParams(paginationToken, scanDirection, num.Value);
        }
    }
}
