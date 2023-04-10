using System.Threading.Tasks;

namespace Thoth.Dashboard.Filter;

public interface IThothDashboardAuthorizationFilter
{
    Task<bool> AuthorizeAsync(ThothDashboardContext thothDashboardContext);
}