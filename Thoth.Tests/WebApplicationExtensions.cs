using Microsoft.AspNetCore.Hosting;

namespace Thoth.Tests;

public static class WebApplicationExtensions
{
    public static IWebHostBuilder ConfigureApplication(this IWebHostBuilder application, Action<IWebHostBuilder> setupAction)
    {
        setupAction.Invoke(application);

        return application;
    }
}