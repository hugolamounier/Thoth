using System.Collections.Generic;
using Thoth.Dashboard.Filter;

namespace Thoth.Dashboard;

public class ThothDashboardOptions
{
    public string RoutePrefix { get; set; } = "/thoth";
    public IEnumerable<string> ClaimsToRegisterOnLog { get; set; } = System.Array.Empty<string>();

    public IEnumerable<IThothDashboardAuthorizationFilter> Authorization { get; set; } =
        new List<IThothDashboardAuthorizationFilter> {new LocalOnlyThothFilter()};
}