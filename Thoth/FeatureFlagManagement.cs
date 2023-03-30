using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Thoth.Interfaces;
using Thoth.Models;

namespace Thoth;

public class FeatureFlagManagement: IFeatureFlagManagement
{
    private readonly IDatabase _dbContext;
    private readonly ILogger<FeatureFlagManagement> _logger;
    private readonly CacheManager _cacheManager;

    public FeatureFlagManagement(IDatabase dbContext, CacheManager cacheManager, ILogger<FeatureFlagManagement> logger)
    {
        _dbContext = dbContext;
        _cacheManager = cacheManager;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string name) =>
        (await GetAsync(name)).Value;

    public async Task<FeatureFlag> GetAsync(string name)
    {
        if (!await CheckIfExists(name))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_ALREADY_EXISTS, name));

        return await _cacheManager.GetOrCreate(name, () => _dbContext.GetAsync(name));
    }

    public async Task<bool> AddAsync(FeatureFlag featureFlag)
    {
        if (await CheckIfExists(featureFlag.Name))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_ALREADY_EXISTS, featureFlag.Name));

        if (featureFlag.Type == FeatureFlagsTypes.Boolean && !string.IsNullOrWhiteSpace(featureFlag.FilterValue))
            throw new ThothException(Messages.ERROR_BOOLEAN_FEATURE_FLAGS_CANT_HAVE_FILTER_VALUE);

        featureFlag = new FeatureFlag
        {
            Name = featureFlag.Name,
            Value = featureFlag.Value,
            FilterValue = featureFlag.FilterValue
        };

        try
        {
            _logger.LogInformation("{Message}",
                string.Format(Messages.INFO_ADDED_FEATURE_FLAG, featureFlag.Name, featureFlag.Value.ToString(), featureFlag.FilterValue));

            var insertResult = await _cacheManager.GetOrCreate(featureFlag.Name, () => _dbContext.AddAsync(featureFlag));

            return insertResult;
        }
        catch(Exception e)
        {
            _logger.LogError("{Message}: {Exception}", Messages.ERROR_WHILE_ADDIND_FEATURE_FLAG,
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    public async Task<bool> UpdateAsync(FeatureFlag featureFlag)
    {
        if (!await CheckIfExists(featureFlag.Name))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_NOT_EXISTS, featureFlag.Name));

        if (featureFlag.Type == FeatureFlagsTypes.Boolean && !string.IsNullOrWhiteSpace(featureFlag.FilterValue))
            throw new ThothException(Messages.ERROR_BOOLEAN_FEATURE_FLAGS_CANT_HAVE_FILTER_VALUE);

        featureFlag = new FeatureFlag
        {
            Name = featureFlag.Name,
            Value = featureFlag.Value,
            FilterValue = featureFlag.FilterValue
        };

        try
        {
            _logger.LogInformation("{Message}",
                string.Format(Messages.INFO_UPDATED_FEATURE_FLAG, featureFlag.Name, featureFlag.Value.ToString(), featureFlag.FilterValue));

            var updateResult = await _dbContext.UpdateAsync(featureFlag);

            if (updateResult)
                await _cacheManager.Update(featureFlag.Name, featureFlag);

            return updateResult;
        }
        catch(Exception e)
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

        if(deleteResult)
            _cacheManager.Remove(name);

        return deleteResult;
    }

    private Task<bool> CheckIfExists(string name) => _dbContext.ExistsAsync(name);
}