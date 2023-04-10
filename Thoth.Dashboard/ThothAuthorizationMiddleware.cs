using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Thoth.Dashboard.Filter;

namespace Thoth.Dashboard;

public class ThothAuthorizationMiddleware
{
    private readonly IWebHostEnvironment _environment;
    private readonly RequestDelegate _next;
    private readonly ThothDashboardOptions _options;

    public ThothAuthorizationMiddleware(RequestDelegate next, ThothDashboardOptions options, IWebHostEnvironment environment)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options;
        _environment = environment;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var thothContext = new ThothDashboardContext(httpContext, _environment);
        var requestPath = httpContext.Request.Path.Value ?? string.Empty;
        var isThothDashboard = requestPath.Contains($"{_options.RoutePrefix}");

        if (!isThothDashboard)
        {
            await _next(httpContext);
            return;
        }

        foreach (var filter in _options.Authorization)
        {
            if (await filter.AuthorizeAsync(thothContext))
                continue;

            httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        if (httpContext.Response.StatusCode == StatusCodes.Status301MovedPermanently)
            return;

        await _next(httpContext);
    }
}