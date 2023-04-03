using Thoth.Dashboard;

namespace Thoth.Tests.Filters;

public class ThothAuthorizationFilter : IThothDashboardAuthorizationFilter
{
    public Task<bool> AuthorizeAsync(ThothDashboardContext thothDashboardContext)
    {
        var requestRemoteIp = thothDashboardContext.Request.RemoteIpAddress;

        return Task.FromResult(requestRemoteIp == "200.100.100.10");
    }
}