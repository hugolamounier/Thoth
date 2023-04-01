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
    public static IServiceCollection AddThoth(this IServiceCollection services, Action<ThothOptions> setupAction)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (setupAction == null)
            throw new ArgumentNullException(nameof(setupAction));

        var options = new ThothOptions();
        setupAction(options);

        services.AddOptions();
        services.Configure(setupAction);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
            throw new ArgumentException(Messages.ERROR_CONNECTION_STRING);

        services.TryAddSingleton<IMemoryCache, MemoryCache>();
        services.TryAddSingleton<CacheManager>();
        services.TryAddSingleton<IThothFeatureManager, ThothFeatureManager>();

        if (options.EnableThothApi)
        {
            services.AddSpaStaticFiles();
            return services;
        }

        var appPartManager = (ApplicationPartManager) services
            .FirstOrDefault(a => a.ServiceType == typeof(ApplicationPartManager))
            ?.ImplementationInstance!;
        var mockingPart = appPartManager.ApplicationParts
            .FirstOrDefault(a => a.Name == "Thoth.Dashboard");

        if (mockingPart != null)
            appPartManager.ApplicationParts.Remove(mockingPart);

        return services;
    }
}