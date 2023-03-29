using System;

namespace Thoth.Models;

public class FeatureFlagOptions
{
    /// <summary>
    /// Defines for how long the feature flags will be cached in memory.
    /// </summary>
    public TimeSpan CacheExpirationInterval { get; set; } = TimeSpan.FromMinutes(5);
}