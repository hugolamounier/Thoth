using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Thoth.Core;

public class ThothExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ThothExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ThothException e)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(e.Message);
        }
    }
}