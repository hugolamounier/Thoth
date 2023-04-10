using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Thoth.Dashboard.Filter;

public class ThothJwtAuthorizationFilter: IThothDashboardAuthorizationFilter
{
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly string _tokenQueryParamName;
    private readonly string? _roleClaimName;
    private readonly IEnumerable<string>? _allowedRoles;
    private readonly string _dashboardRootPath;
    private readonly CookieOptions? _cookieOptions;

    public ThothJwtAuthorizationFilter(
        TokenValidationParameters tokenValidationParameters,
        string tokenQueryParamName = "accessToken",
        string? roleClaimName = ClaimTypes.Role,
        IEnumerable<string>? allowedRoles = null,
        CookieOptions? cookieOptions = null,
        string dashboardRootPath = "/thoth")
    {
        _tokenValidationParameters = tokenValidationParameters;
        _tokenQueryParamName = tokenQueryParamName;
        _roleClaimName = roleClaimName;
        _allowedRoles = allowedRoles;
        _dashboardRootPath = dashboardRootPath;
        _cookieOptions = cookieOptions ?? new CookieOptions
        {
            Secure = true,
            HttpOnly = true
        };
    }

    public async Task<bool> AuthorizeAsync(ThothDashboardContext thothDashboardContext)
    {
        string? jwtToken;

        if (thothDashboardContext.HttpContext.Request.Query.ContainsKey(_tokenQueryParamName))
        {
            jwtToken = thothDashboardContext.HttpContext.Request.Query[_tokenQueryParamName].FirstOrDefault();
            var isAuthorized = await IsAuthorized(jwtToken, thothDashboardContext.HttpContext);

            if (!isAuthorized)
                return false;

            SetCookie(jwtToken, thothDashboardContext.HttpContext);

            thothDashboardContext.HttpContext.Response.StatusCode = StatusCodes.Status302Found;
            thothDashboardContext.HttpContext.Response.Headers["Location"] = _dashboardRootPath;

            return true;
        }

        jwtToken = thothDashboardContext.HttpContext.Request.Cookies["_thothCookie"];
        return await IsAuthorized(jwtToken, thothDashboardContext.HttpContext);
    }

    private Task<bool> IsAuthorized(string? jwtToken, HttpContext httpContext)
    {
        if (string.IsNullOrEmpty(jwtToken))
            return Task.FromResult(false);
        
        var tokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal? jwtSecurityToken;

        try
        {
            jwtSecurityToken = tokenHandler.ValidateToken(jwtToken, _tokenValidationParameters, out var outToken);
            _cookieOptions!.Expires = outToken.ValidTo;
        }
        catch
        {
            return Task.FromResult(false);
        }


        if (jwtSecurityToken is null)
            return Task.FromResult(false);

        httpContext.User = new ClaimsPrincipal(jwtSecurityToken);
        var isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        if (_allowedRoles is null)
            return Task.FromResult(isAuthenticated);

        return Task.FromResult(isAuthenticated && jwtSecurityToken.Claims.Any(t => t.Type == _roleClaimName &&
                                                                               _allowedRoles.Contains(t.Value)));
    }

    private void SetCookie(string? jwtToken, HttpContext httpContext)
    {
        if (jwtToken is null) return;

        httpContext.Response.Cookies.Append("_thothCookie", jwtToken, _cookieOptions!);
    }
}