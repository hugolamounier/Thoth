#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Thoth.Core.Models;
using Thoth.Core.Models.Entities;

namespace Thoth.Core;

public class CacheManager
{
    private readonly IMemoryCache _memoryCache;
    private readonly ThothOptions _options;

    public CacheManager(IMemoryCache memoryCache, IOptions<ThothOptions> options)
    {
        _options = options.Value;
        _memoryCache = memoryCache;
    }

    public async Task<FeatureManager?> GetOrCreateAsync(object cacheKey, Func<Task<FeatureManager?>> action)
    {
        if (_options.EnableCaching is false)
            return await action.Invoke();

        return await _memoryCache.GetOrCreateAsync(cacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = _options.CacheExpiration;
            cacheEntry.SlidingExpiration = _options.CacheSlidingExpiration;

            return await action.Invoke();
        });
    }

    public FeatureManager? GetIfExists(object cacheKey)
    {
        if (_options.EnableCaching is false)
            return null;

        var cacheKeyExists = _memoryCache.TryGetValue(cacheKey, out FeatureManager? cachedValue);

        return cacheKeyExists ? cachedValue : null;
    }

    public void Update(object cacheKey, FeatureManager featureManager)
    {
        if (_options.EnableCaching is false)
            return;

        var cachedValue = GetIfExists(cacheKey);

        if (cachedValue != null)
            _memoryCache.Remove(cacheKey);

        cachedValue = featureManager;
        cachedValue.UpdatedAt = DateTime.UtcNow;

        _memoryCache.Set(cacheKey, cachedValue, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.CacheExpiration,
            SlidingExpiration = _options.CacheSlidingExpiration
        });
    }

    public void Remove(object cacheKey)
    {
        if (_options.EnableCaching is false)
            return;

        _memoryCache.Remove(cacheKey);
    }
}