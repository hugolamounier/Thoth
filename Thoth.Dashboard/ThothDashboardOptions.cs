using Thoth.Dashboard.Audit;
using Thoth.Dashboard.Filter;

namespace Thoth.Dashboard;

public class ThothDashboardOptions
{
    public string RoutePrefix { get; set; } = "/thoth";
    public IThothManagerAudit? ThothManagerAudit { get; set; }
    public IThothDashboardAuthorizationFilter? Authorization { get; set; }
}