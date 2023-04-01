﻿using System.Diagnostics.CodeAnalysis;
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
using Thoth.Dashboard.Api;
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

                if (options.ConnectionString is null)
                    throw new ArgumentException(Messages.ERROR_CONNECTION_STRING);
                services.TryAddSingleton<IDatabase, SqlServerDatabase>();

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        services.TryAddSingleton<IThothFeatureManager, ThothFeatureManager>();

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

    public static IApplicationBuilder UseThothDashboard(this WebApplication app, Action<ThothDashboardOptions>? setupAction = null)
    {
        using var scope = app.Services.CreateScope();
        var options = (ThothDashboardOptions?) scope.ServiceProvider.GetRequiredService<IOptions<ThothDashboardOptions>>().Value;
        var thothOptions = (ThothOptions?) scope.ServiceProvider.GetRequiredService<IOptions<ThothOptions>>().Value;
        setupAction?.Invoke(options);

        if(!thothOptions?.EnableThothApi ?? true)
            throw new ArgumentException(Messages.ERROR_CAN_NOT_USE_THOTH_DASHBOARD);

        options ??= new ThothDashboardOptions();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new EmbeddedFileProvider(typeof(ThothDashboardOptions).Assembly, "Thoth.Dashboard.wwwroot")
        });

        app.Map(options.RoutePrefix, mappedSpa =>
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

        app.UseMiddleware<ThothExceptionMiddleware>();

        return app.InjectThothDashboardRoutes(scope, options.RoutePrefix);
    }
}