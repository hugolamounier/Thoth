using Microsoft.Extensions.Options;

namespace Thoth.Dashboard;

public class ThothDashboardOptions : IOptions<ThothDashboardOptions>
{
    public string RoutePrefix { get; set; } = "/thoth";

    public ThothDashboardOptions Value { get; }
}