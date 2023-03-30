using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Thoth;
using Thoth.Interfaces;
using Thoth.Models;
using Thoth.SQLServer;

namespace Microsoft.Extensions.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtension
{
    public static IServiceCollection AddThoth(this IServiceCollection services, Action<FeatureFlagOptions> setupAction)
    {
        if(services == null)
            throw new ArgumentNullException(nameof(services));

        if(setupAction == null)
            throw new ArgumentNullException(nameof(setupAction));

        var options = new FeatureFlagOptions();
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
                services.TryAddScoped<IDatabase, SqlServerDatabase>();

                break;
            case DatabaseTypes.MongoDb:
                throw new NotImplementedException();
            default:
                throw new ArgumentOutOfRangeException();
        }

        services.TryAddSingleton<IFeatureFlagManagement, FeatureFlagManagement>();

        return services;
    }
}