using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Thoth.Core.Models;

public class ThothDashboardRequest
{
    private readonly HttpContext _httpContext;

    public ThothDashboardRequest([NotNull] HttpContext context)
    {
        _httpContext = context ?? throw new ArgumentNullException(nameof(context));
    }

    public string Method => _httpContext.Request.Method;

    public string Path => _httpContext.Request.Path.Value;

    public string PathBase => _httpContext.Request.PathBase.Value;

    public string LocalIpAddress => _httpContext.Connection.LocalIpAddress?.ToString();

    public string RemoteIpAddress => _httpContext.Connection.RemoteIpAddress?.ToString();
}