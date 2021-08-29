namespace BWF.Api.Host.Middleware
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Builder;

    public static class ErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandlerMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }

    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext).ConfigureAwait(false);
            }
            catch (NotImplementedException ex)
            {
                await TransformExceptionToNotImplemented(httpContext, ex);
            }
            catch (Exception ex)
            {
                await TransfromExceptionToServerError(httpContext, ex);
            }
        }

        private async Task TransformExceptionToNotImplemented(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
            await context.Response.WriteAsync(ex.Message);
        }

        private async Task TransfromExceptionToServerError(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync(ex.Message);
        }
    }
}
