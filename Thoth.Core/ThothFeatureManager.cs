﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.Core;

public class ThothFeatureManager : IThothFeatureManager
{
    private readonly CacheManager _cacheManager;
    private readonly IDatabase _dbContext;
    private readonly ILogger<ThothFeatureManager> _logger;

    public ThothFeatureManager(IDatabase dbContext, CacheManager cacheManager, ILogger<ThothFeatureManager> logger)
    {
        _dbContext = dbContext;
        _cacheManager = cacheManager;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string name)
    {
        return (await GetAsync(name)).Value;
    }

    public async Task<FeatureFlag> GetAsync(string name)
    {
        if (!await CheckIfExists(name))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_ALREADY_EXISTS, name));

        return await _cacheManager.GetOrCreateAsync(name, () => _dbContext.GetAsync(name));
    }

    public async Task<IEnumerable<FeatureFlag>> GetAllAsync()
    {
        var featureFlags = await _dbContext.GetAllAsync();

        foreach (var featureFlag in featureFlags)
            _ = await _cacheManager.GetOrCreateAsync(featureFlag.Name, () => Task.FromResult(featureFlag));

        return featureFlags;
    }

    public async Task<bool> AddAsync(FeatureFlag featureFlag)
    {
        if (await CheckIfExists(featureFlag.Name))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_ALREADY_EXISTS, featureFlag.Name));

        if (featureFlag.Type == FeatureFlagsTypes.Boolean && !string.IsNullOrWhiteSpace(featureFlag.FilterValue))
            throw new ThothException(Messages.ERROR_BOOLEAN_FEATURE_FLAGS_CANT_HAVE_FILTER_VALUE);

        try
        {
            _logger.LogInformation("{Message}",
                string.Format(Messages.INFO_ADDED_FEATURE_FLAG, featureFlag.Name, featureFlag.Value.ToString(), featureFlag.FilterValue));

            var insertResult = await _dbContext.AddAsync(featureFlag);

            if (insertResult)
                await _cacheManager.GetOrCreateAsync(featureFlag.Name, () => Task.FromResult(featureFlag));

            return insertResult;
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}: {Exception}", Messages.ERROR_WHILE_ADDIND_FEATURE_FLAG,
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(FeatureFlag featureFlag)
    {
        var dbFeatureFlag = await GetAsync(featureFlag.Name);

        if (featureFlag.Type == FeatureFlagsTypes.Boolean && !string.IsNullOrWhiteSpace(featureFlag.FilterValue))
            throw new ThothException(Messages.ERROR_BOOLEAN_FEATURE_FLAGS_CANT_HAVE_FILTER_VALUE);

        try
        {
            _logger.LogInformation("{Message}",
                string.Format(Messages.INFO_UPDATED_FEATURE_FLAG, featureFlag.Name, featureFlag.Value.ToString(), featureFlag.FilterValue));

            featureFlag.CreatedAt = dbFeatureFlag.CreatedAt;
            featureFlag.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _dbContext.UpdateAsync(featureFlag);

            if (updateResult)
                await _cacheManager.UpdateAsync(featureFlag.Name, featureFlag);

            return updateResult;
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}: {Exception}", Messages.ERROR_WHILE_ADDIND_FEATURE_FLAG,
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string name)
    {
        if (!await CheckIfExists(name))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, name));

        var deleteResult = await _dbContext.DeleteAsync(name);

        if (deleteResult)
            _cacheManager.Remove(name);

        return deleteResult;
    }

    private async Task<bool> CheckIfExists(string name)
    {
        var cachedValue = _cacheManager.GetIfExistsAsync(name);
        if (cachedValue != null)
            return true;

        return await _dbContext.ExistsAsync(name);
    }
}