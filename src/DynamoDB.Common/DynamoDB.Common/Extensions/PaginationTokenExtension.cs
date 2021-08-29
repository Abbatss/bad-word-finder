namespace DynamoDB.Common.Extensions
{
    public static class PaginationTokenExtension
    {
        public static bool IsEndOrBegin(this string paginationToken)
        {
            return string.IsNullOrWhiteSpace(paginationToken) || paginationToken == "{}";
        }
    }
}
