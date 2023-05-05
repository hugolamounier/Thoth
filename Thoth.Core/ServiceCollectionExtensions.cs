using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.Core;

/// <summary>
///     Contains extension methods to <see cref="IServiceCollection" /> for configuring consistence services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddThoth(this IServiceCollection services, Action<ThothOptions> setupAction)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (setupAction == null)
            throw new ArgumentNullException(nameof(setupAction));

        var options = new ThothOptions();
        setupAction(options);

        services.AddOptions();
        services.Configure(setupAction);

        options.Extensions.ForEach(extension => extension(services));

        services.AddHttpContextAccessor();
        services.TryAddSingleton<IMemoryCache, MemoryCache>();
        services.TryAddSingleton<CacheManager>();
        services.TryAddScoped<IThothFeatureManager, ThothFeatureManager>();

        if (options.EnableThothApi)
            services.AddSpaStaticFiles();

        var appPartManager = (ApplicationPartManager) services
            .FirstOrDefault(a => a.ServiceType == typeof(ApplicationPartManager))
            ?.ImplementationInstance!;
        var dashboardAppPart = appPartManager?.ApplicationParts
            .FirstOrDefault(a => a.Name == "Thoth.Dashboard");

        if (dashboardAppPart != null)
            appPartManager.ApplicationParts.Remove(dashboardAppPart);
    }
}