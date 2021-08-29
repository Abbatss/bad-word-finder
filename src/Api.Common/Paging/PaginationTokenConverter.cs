using System;

namespace Api.Common.Paging
{
    public static class PaginationTokenConverter
    {
        public static string ToBase64String(this string paginationToken)
        {
            byte[] byt = System.Text.Encoding.UTF8.GetBytes(paginationToken);
            return Convert.ToBase64String(byt);
        }

        public static string FromBase64String(this string base64)
        {
            try
            {
                return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            }
            catch
            {
                throw new ArgumentException("String value is not valid base 64 string", nameof(base64));
            }
        }
    }
}