using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Thoth.Core.Models;

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

    public Task<FeatureFlag> GetOrCreateAsync(string cacheKey, Func<Task<FeatureFlag>> action)
    {
        if (_options.EnableCaching is false)
            return action.Invoke();

        return _memoryCache.GetOrCreateAsync(cacheKey, cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = _options.CacheExpiration;
            cacheEntry.SlidingExpiration = _options.CacheSlidingExpiration;

            return action.Invoke();
        });
    }

    public FeatureFlag GetIfExistsAsync(string cacheKey)
    {
        if (_options.EnableCaching is false)
            return null;

        var cacheKeyExists = _memoryCache.TryGetValue(cacheKey, out FeatureFlag cachedValue);

        return cacheKeyExists ? cachedValue : null;
    }

    public async Task UpdateAsync(string cacheKey, FeatureFlag featureFlag)
    {
        if (_options.EnableCaching is false)
            return;

        var cachedValue = GetIfExistsAsync(cacheKey);

        if (cachedValue == null)
        {
            featureFlag = new FeatureFlag
            {
                Name = cacheKey,
                Type = featureFlag.Type,
                Value = featureFlag.Value,
                FilterValue = featureFlag.FilterValue,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await GetOrCreateAsync(cacheKey, () => Task.FromResult(featureFlag));
            return;
        }

        cachedValue.Value = featureFlag.Value;
        cachedValue.FilterValue = featureFlag.FilterValue;

        _memoryCache.Remove(cacheKey);
        _memoryCache.Set(cacheKey, cachedValue, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.CacheExpiration,
            SlidingExpiration = _options.CacheSlidingExpiration
        });
    }

    public void Remove(string cacheKey)
    {
        if (_options.EnableCaching is false)
            return;

        _memoryCache.Remove(cacheKey);
    }
}