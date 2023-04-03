using System.Threading.Tasks;

namespace Thoth.Dashboard;

public interface IThothDashboardAuthorizationFilter
{
    Task<bool> AuthorizeAsync(ThothDashboardContext thothDashboardContext);
}