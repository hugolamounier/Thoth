using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;
using Thoth.Core.Models.Entities;
using Thoth.Core.Models.Enums;

namespace Thoth.Core;

public class ThothFeatureManager: IThothFeatureManager
{
    private readonly CacheManager _cacheManager;
    private readonly IDatabase _dbContext;
    private readonly ILogger<ThothFeatureManager> _logger;
    private readonly ThothOptions _thothOptions;

    public ThothFeatureManager(
        CacheManager cacheManager,
        ILogger<ThothFeatureManager> logger,
        IOptions<ThothOptions> thothOptions)
    {
        _cacheManager = cacheManager;
        _logger = logger;
        _dbContext = thothOptions.Value.DatabaseProvider;
        _thothOptions = thothOptions.Value;
    }

    public async Task<bool> IsEnabledAsync(string featureName)
    {
        try
        {
            var featureFlag = await GetAsync(featureName);

            return await EvaluateAsync(featureFlag);
        }
        catch (Exception e)
            when (e.Message == string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, featureName) &&
                  _thothOptions.ShouldReturnFalseWhenNotExists)
        {
            _logger.LogInformation("{Message}", string.Format(Messages.INFO_NON_EXISTENT_FLAG_REQUESTED, featureName));
            return false;
        }
    }

    public async Task<T> GetEnvironmentValueAsync<T>(string featureName)
    {
        if (await IsEnabledAsync(featureName) is false)
            throw new ThothException(Messages.ERROR_CAN_NOT_GET_DISABLED_FEATURE);

        var feature = await GetAsync(featureName);

        if (feature.Type is not FeatureTypes.EnvironmentVariable)
            throw new ThothException(string.Format(Messages.ERROR_WRONG_FEATURE_TYPE, featureName, "EnvironmentVariable"));

        return (T) Convert.ChangeType(feature.Value, typeof(T));
    }

    public async Task<FeatureManager> GetAsync(string featureId)
    {
        if (!await CheckIfExistsAsync(featureId))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, featureId));

        return await _cacheManager.GetOrCreateAsync(featureId, () => _dbContext.GetAsync(featureId));
    }

    public async Task<IEnumerable<FeatureManager>> GetAllAsync()
    {
        var featureFlags = await _dbContext.GetAllAsync();

        foreach (var featureFlag in featureFlags)
            _ = await _cacheManager.GetOrCreateAsync(featureFlag.Name, () => Task.FromResult(featureFlag));

        return featureFlags;
    }

    public async Task<bool> AddAsync(FeatureManager featureManager)
    {
        try
        {
            if (await CheckIfExistsAsync(featureManager.Name))
                throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_ALREADY_EXISTS, featureManager.Name));

            var insertResult = await _dbContext.AddAsync(featureManager);

            if (insertResult)
                await _cacheManager.GetOrCreateAsync(featureManager.Name, () => Task.FromResult(featureManager));

            return insertResult;
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}: {Exception}", Messages.ERROR_WHILE_ADDIND_FEATURE_FLAG,
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(FeatureManager featureManager)
    {
        try
        {
            var dbFeatureFlag = await GetAsync(featureManager.Name);

            featureManager.CreatedAt = dbFeatureFlag.CreatedAt;
            featureManager.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _dbContext.UpdateAsync(featureManager);

            if (updateResult)
                _cacheManager.UpdateAsync(featureManager.Name, featureManager);

            return updateResult;
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}: {Exception}", string.Format(Messages.ERROR_WHILE_UPDATING_FEATURE_FLAG, featureManager.Name),
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string featureName)
    {
        try
        {
            if (!await CheckIfExistsAsync(featureName))
                throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, featureName));

            var deleteResult = await _dbContext.DeleteAsync(featureName);

            if (deleteResult)
                _cacheManager.Remove(featureName);

            return deleteResult;
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}: {Exception}", string.Format(Messages.ERROR_WHILE_DELETING_FEATURE_FLAG, featureName),
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    private async Task<bool> CheckIfExistsAsync(string featureId)
    {
        var cachedValue = _cacheManager.GetIfExistsAsync(featureId);
        if (cachedValue != null)
            return true;

        return await _dbContext.ExistsAsync(featureId);
    }

    private async Task<bool> EvaluateAsync(FeatureManager featureManager)
    {
        return featureManager.SubType switch
        {
            FeatureFlagsTypes.Boolean => featureManager.Enabled,
            FeatureFlagsTypes.PercentageFilter =>
                featureManager.Enabled && await featureManager.EvaluatePercentageFlag(_logger),
            _ => featureManager.Enabled
        };
    }
}