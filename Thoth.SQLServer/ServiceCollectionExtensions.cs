using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Thoth.Core.Interfaces;

namespace Thoth.SQLServer;

/// <summary>
///     Contains extension methods to <see cref="IServiceCollection" /> for configuring consistence services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseSqlServer(this IServiceCollection services)
    {
        services.TryAddSingleton<IDatabase, SqlServerDatabase>();
        return services;
    }
}