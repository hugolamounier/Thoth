using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Thoth.Models;

namespace Thoth;

public class CacheManager
{
    private readonly IMemoryCache _memoryCache;
    private readonly FeatureFlagOptions _options;

    public CacheManager(IMemoryCache memoryCache, FeatureFlagOptions options)
    {
        _options = options;
        _memoryCache = memoryCache;
    }

    public async Task<T> GetOrCreate<T>(string cacheKey, Func<Task<T>> action)
    {
        return await _memoryCache.GetOrCreateAsync(cacheKey, cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = _options.CacheExpiration;
            cacheEntry.SlidingExpiration = _options.CacheSlidingExpiration;

            return action.Invoke();
        });
    }

    public async Task Update(string cacheKey, FeatureFlag featureFlag)
    {
        var cacheKeyExists = _memoryCache.TryGetValue(cacheKey, out FeatureFlag cachedValue);

    }

    public void Remove(string cacheKey) =>  _memoryCache.Remove(cacheKey);
}