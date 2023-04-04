using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.Core;

public class ThothFeatureManager : IThothFeatureManager
{
    private readonly CacheManager _cacheManager;
    private readonly IDatabase _dbContext;
    private readonly ILogger<ThothFeatureManager> _logger;
    private readonly ThothOptions _thothOptions;

    public ThothFeatureManager(
        IDatabase dbContext,
        CacheManager cacheManager,
        ILogger<ThothFeatureManager> logger,
        IOptions<ThothOptions> thothOptions)
    {
        _dbContext = dbContext;
        _cacheManager = cacheManager;
        _logger = logger;
        _thothOptions = thothOptions.Value;
    }

    public async Task<bool> IsEnabledAsync(string name)
    {
        try
        {
            var featureFlag = await GetAsync(name);

            return await EvaluateAsync(featureFlag);
        }
        catch (Exception e)
        when (e.Message == string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, name) &&
              _thothOptions.ShouldReturnFalseWhenNotExists)
        {
            _logger.LogInformation("{Message}", string.Format(Messages.INFO_NON_EXISTENT_FLAG_REQUESTED, name));
            return false;
        }
    }

    public async Task<FeatureFlag> GetAsync(string name)
    {
        if (!await CheckIfExistsAsync(name))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, name));

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
        try
        {
            if (await CheckIfExistsAsync(featureFlag.Name))
                throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_ALREADY_EXISTS, featureFlag.Name));

            var insertResult = await _dbContext.AddAsync(featureFlag);

            if (insertResult)
                await _cacheManager.GetOrCreateAsync(featureFlag.Name, () => Task.FromResult(featureFlag));

            _logger.LogInformation("{Message}",
                string.Format(Messages.INFO_ADDED_FEATURE_FLAG, featureFlag.Name, featureFlag.Value.ToString(),
                    featureFlag.FilterValue));

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
        try
        {
            var dbFeatureFlag = await GetAsync(featureFlag.Name);

            featureFlag.CreatedAt = dbFeatureFlag.CreatedAt;
            featureFlag.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _dbContext.UpdateAsync(featureFlag);

            if (updateResult)
                await _cacheManager.UpdateAsync(featureFlag.Name, featureFlag);

            _logger.LogInformation("{Message}",
                string.Format(Messages.INFO_UPDATED_FEATURE_FLAG, featureFlag.Name, featureFlag.Value.ToString(), featureFlag.FilterValue));

            return updateResult;
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}: {Exception}", string.Format(Messages.ERROR_WHILE_UPDATING_FEATURE_FLAG, featureFlag.Name),
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string name)
    {
        try
        {
            if (!await CheckIfExistsAsync(name))
                throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, name));

            var deleteResult = await _dbContext.DeleteAsync(name);

            if (deleteResult)
                _cacheManager.Remove(name);

            _logger.LogInformation("{Message}",
                string.Format(Messages.INFO_DELETED_FEATURE_FLAG, name));

            return deleteResult;
        }
        catch (Exception e)
        {
            _logger.LogError("{Message}: {Exception}", string.Format(Messages.ERROR_WHILE_DELETING_FEATURE_FLAG, name),
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    private async Task<bool> CheckIfExistsAsync(string name)
    {
        var cachedValue = _cacheManager.GetIfExistsAsync(name);
        if (cachedValue != null)
            return true;

        return await _dbContext.ExistsAsync(name);
    }

    private async Task<bool> EvaluateAsync(FeatureFlag featureFlag)
    {
        return featureFlag.Type switch
        {
            FeatureFlagsTypes.Boolean => featureFlag.Value,
            FeatureFlagsTypes.PercentageFilter =>
                featureFlag.Value && await featureFlag.EvaluatePercentageFlag(_logger),
            _ => false
        };
    }
}