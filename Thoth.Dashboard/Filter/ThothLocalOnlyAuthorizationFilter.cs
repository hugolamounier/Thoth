using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Thoth.Dashboard.Filter;

[ExcludeFromCodeCoverage]
public class ThothLocalOnlyAuthorizationFilter : IThothDashboardAuthorizationFilter
{
    public Task<bool> AuthorizeAsync(ThothDashboardContext thothDashboardContext)
    {
        if (thothDashboardContext.WebHostEnvironment.EnvironmentName == "Testing")
            return Task.FromResult(true);

        return Task.FromResult(!string.IsNullOrEmpty(thothDashboardContext.Request.RemoteIpAddress) &&
                               (thothDashboardContext.Request.RemoteIpAddress is "127.0.0.1" or "::1" ||
                                thothDashboardContext.Request.RemoteIpAddress ==
                                thothDashboardContext.Request.LocalIpAddress));
    }
}