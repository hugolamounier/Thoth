using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Thoth.Dashboard.Filter;

public class ThothJwtAuthorizationFilter: IThothDashboardAuthorizationFilter
{
    private readonly string _tokenQueryParamName;
    private readonly string? _roleClaimName;
    private readonly IEnumerable<string>? _allowedRoles;

    public ThothJwtAuthorizationFilter(string tokenQueryParamName = "accessToken", string? roleClaimName = null, IEnumerable<string>? allowedRoles = null)
    {
        _tokenQueryParamName = tokenQueryParamName;
        _roleClaimName = roleClaimName;
        _allowedRoles = allowedRoles;
    }

    public Task<bool> AuthorizeAsync(ThothDashboardContext thothDashboardContext)
    {
        string? jwtToken;

        if (thothDashboardContext.HttpContext.Request.Query.ContainsKey(_tokenQueryParamName))
        {
            jwtToken = thothDashboardContext.HttpContext.Request.Query[_tokenQueryParamName].FirstOrDefault();
            SetCookie(jwtToken, thothDashboardContext.HttpContext);

            return IsAuthorized(jwtToken, thothDashboardContext.HttpContext);
        }

        jwtToken = thothDashboardContext.HttpContext.Request.Cookies["_thothCookie"];
        return IsAuthorized(jwtToken, thothDashboardContext.HttpContext);
    }

    private Task<bool> IsAuthorized(string? jwtToken, HttpContext httpContext)
    {
        if (string.IsNullOrEmpty(jwtToken))
            return Task.FromResult(false);

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

        if (_roleClaimName is null || _allowedRoles is null)
            return Task.FromResult(httpContext.User.Identity?.IsAuthenticated ?? false);

        return Task.FromResult(jwtSecurityToken.Claims.Any(t => t.Type == _roleClaimName &&
                                                                _allowedRoles.Contains(t.Value)));
    }

    private static void SetCookie(string? jwtToken, HttpContext httpContext)
    {
        if (jwtToken is null) return;

        httpContext.Response.Cookies.Append("_thothCookie", jwtToken, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(30)
        });
    }
}