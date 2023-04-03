using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Thoth.Core.Models;

namespace Thoth.Dashboard;

public class ThothDashboardContext
{
    public ThothDashboardContext(HttpContext httpContext, IWebHostEnvironment environment)
    {
        Request = new ThothDashboardRequest(httpContext);
        WebHostEnvironment = environment;
    }

    public IWebHostEnvironment WebHostEnvironment { get; }
    public ThothDashboardRequest Request { get; }
}