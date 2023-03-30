using System;

namespace Thoth.Models;

public class FeatureFlagOptions
{
    /// <summary>
    /// Defines for how long the feature flags will be cached in memory.
    /// </summary>
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Defines for how long the feature flags will be cached in memory without any accesses
    /// </summary>
    public TimeSpan CacheSlidingExpiration { get; set; } = TimeSpan.FromDays(1);
}