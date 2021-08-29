namespace Api.Common.Paging
{
    public class PagingParams
    {
        private const int defaultLimit = 100;
        public string Cursor { get; set; }
        public ScanDirections ScanDirection { get; set; }
        public int Limit { get; set; }
        public PagingParams() : this((string)null)
        {
        }

        public PagingParams(string cusor) : this(cusor, ScanDirections.Forward)
        {
        }

        public PagingParams(string cusor, ScanDirections scanDirection) : this(cusor, scanDirection, defaultLimit)
        {
        }

        public PagingParams(string cursor, ScanDirections scanDirection, int limit)
        {
            Cursor = cursor;
            ScanDirection = scanDirection;
            Limit = limit;
        }

        public PagingParams(PagingParams pagingParam) : this(pagingParam.Cursor, pagingParam.ScanDirection, pagingParam.Limit)
        {
        }
    }
}
