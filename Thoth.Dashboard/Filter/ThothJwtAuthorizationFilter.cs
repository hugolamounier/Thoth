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
    private readonly CookieOptions? _cookieOptions;

    public ThothJwtAuthorizationFilter(
        TokenValidationParameters tokenValidationParameters,
        string tokenQueryParamName = "accessToken",
        string? roleClaimName = ClaimTypes.Role,
        IEnumerable<string>? allowedRoles = null,
        CookieOptions? cookieOptions = null)
    {
        _tokenValidationParameters = tokenValidationParameters;
        _tokenQueryParamName = tokenQueryParamName;
        _roleClaimName = roleClaimName;
        _allowedRoles = allowedRoles;
        _cookieOptions = cookieOptions ?? new CookieOptions
        {
            Secure = true,
            HttpOnly = true
        };
    }

    public async Task<bool> AuthorizeAsync(ThothDashboardContext thothDashboardContext)
    {
        string? jwtToken;
        bool isAuthorized;

        if (thothDashboardContext.HttpContext.Request.Query.ContainsKey(_tokenQueryParamName))
        {
            jwtToken = thothDashboardContext.HttpContext.Request.Query[_tokenQueryParamName].FirstOrDefault();
            isAuthorized = await IsAuthorized(jwtToken, thothDashboardContext.HttpContext);

            if(isAuthorized)
                SetCookie(jwtToken, thothDashboardContext.HttpContext);

            return isAuthorized;
        }

        jwtToken = thothDashboardContext.HttpContext.Request.Cookies["_thothCookie"];
        isAuthorized = await IsAuthorized(jwtToken, thothDashboardContext.HttpContext);

        if(isAuthorized)
            SetCookie(jwtToken, thothDashboardContext.HttpContext);

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