using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Thoth.Core;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;
using Thoth.Dashboard;
using Thoth.SQLServer;

namespace Thoth.DependencyInjection;

/// <summary>
/// Contains extension methods to <see cref="IServiceCollection" /> for configuring consistence services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddThoth(this IServiceCollection services, Action<ThothOptions> setupAction)
    {
        if(services == null)
            throw new ArgumentNullException(nameof(services));

        if(setupAction == null)
            throw new ArgumentNullException(nameof(setupAction));

        var options = new ThothOptions();
        setupAction(options);

        services.AddOptions();
        services.Configure(setupAction);
        services.TryAddSingleton<IMemoryCache, MemoryCache>();
        services.TryAddSingleton<CacheManager>();

        switch (options.UseDatabase)
        {
            case DatabaseTypes.SqlServer:

                if (options.SqlServerConnectionString is null)
                    throw new ThothException(Messages.ERROR_SQL_SERVER_IS_REQUIRED);
                services.TryAddSingleton<IDatabase, SqlServerDatabase>();

                break;
            case DatabaseTypes.MongoDb:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }

        services.TryAddSingleton<IFeatureFlagManagement, FeatureFlagManagement>();

        if (options.EnableThothApi)
        {
            services.AddSpaStaticFiles();
            return services;
        }

        var appPartManager = (ApplicationPartManager)services
            .FirstOrDefault(a => a.ServiceType == typeof(ApplicationPartManager))
            ?.ImplementationInstance!;
        var mockingPart = appPartManager.ApplicationParts
            .FirstOrDefault(a => a.Name == "Thoth.Dashboard");

        if(mockingPart != null)
            appPartManager.ApplicationParts.Remove(mockingPart);

        return services;
    }

    public static IApplicationBuilder UseThothDashboard(this IApplicationBuilder app, Action<ThothDashboardOptions>? setupAction = null)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<ThothDashboardOptions>>().Value;
        var thothOptions = scope.ServiceProvider.GetRequiredService<IOptions<ThothOptions>>().Value;
        setupAction?.Invoke(options);

        if(!thothOptions.EnableThothApi)
            throw new ThothException(Messages.ERROR_CAN_NOT_USE_THOTH_DASHBOARD);

        if (setupAction is null)
            options = new ThothDashboardOptions();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new EmbeddedFileProvider(typeof(ThothDashboardOptions).Assembly, "Thoth.Dashboard.wwwroot")
        });

        app.Map(options.RoutePrefix, mappedSpa=>
        {
            mappedSpa.UseSpa(spa =>
            {
                spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
                {
                    FileProvider = new EmbeddedFileProvider(typeof(ThothDashboardOptions).Assembly, "Thoth.Dashboard.wwwroot")
                };
                spa.Options.SourcePath = "wwwroot";
            });
        });

        return app;
    }
}