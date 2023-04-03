using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Thoth.Core;
using Thoth.Core.Models;
using Thoth.Dashboard.Api;

namespace Thoth.Dashboard;

/// <summary>
///     Contains extension methods to <see cref="IServiceCollection" /> for configuring consistence services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseThothDashboard(this WebApplication app, Action<ThothDashboardOptions>? setupAction = null)
    {
        using var scope = app.Services.CreateScope();
        var options = (ThothDashboardOptions?) scope.ServiceProvider.GetRequiredService<IOptions<ThothDashboardOptions>>().Value;
        var thothOptions = (ThothOptions?) scope.ServiceProvider.GetRequiredService<IOptions<ThothOptions>>().Value;

        options ??= new ThothDashboardOptions();
        setupAction?.Invoke(options);

        if (!thothOptions?.EnableThothApi ?? true)
            throw new ArgumentException(Messages.ERROR_CAN_NOT_USE_THOTH_DASHBOARD);

        app.UseMiddleware<ThothAuthorizationMiddleware>(options);

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