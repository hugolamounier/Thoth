using System;
using Microsoft.Extensions.Options;

namespace Thoth.Models;

public class FeatureFlagOptions: IOptions<FeatureFlagOptions>
{
    /// <summary>
    /// Whether feature flags should be cached, enabled by default
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Sets the database type that will store the feature flags
    /// </summary>
    public DatabaseTypes UseDatabase { get; set; }

    /// <summary>
    /// SQL Server connection string
    /// </summary>
    public string SqlServerConnectionString { get; set; }

    /// <summary>
    /// Defines for how long the feature flags will be cached in memory.
    /// Distributed applications should set shorten durations.
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Defines for how long the feature flags will be cached in memory without any accesses
    /// Distributed applications should set shorten durations.
    /// </summary>
    public TimeSpan CacheSlidingExpiration { get; set; } = TimeSpan.FromDays(1);

    public FeatureFlagOptions Value => this;
}