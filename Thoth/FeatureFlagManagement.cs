using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Thoth.Interfaces;
using Thoth.Models;

namespace Thoth;

public class FeatureFlagManagement: IFeatureFlagManagement
{
    private readonly IDatabase _dbContext;
    private readonly ILogger<FeatureFlagManagement> _logger;
    private ConcurrentBag<IDataflowBlock> Actions { get; set; } = new();

    public FeatureFlagManagement(IDatabase dbContext, ILogger<FeatureFlagManagement> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> AddAsync(string name, FeatureFlagsTypes type, bool value)
    {
        if (await CheckIfExists(name))
            throw new ThothException(string.Format(Messages.ERROR_FEATURE_FLAG_ALREADY_EXISTS, name));

        var featureFlag = new FeatureFlag { Name = name, Type = type, Value = value };

        try
        {
            _logger.LogInformation("{Message}", string.Format(Messages.INFO_ADDED_FEATURE_FLAG, name, value.ToString()));
            return await _dbContext.AddAsync(featureFlag);
        }
        catch(Exception e)
        {
            _logger.LogError("{Message}: {Exception}", Messages.ERROR_WHILE_ADDIND_FEATURE_FLAG,
                e.InnerException?.Message ?? e.Message);
            throw;
        }
    }

    public Task UpdateAsync(string name, bool value)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteAsync(string name)
    {
        throw new System.NotImplementedException();
    }

    private Task<bool> CheckIfExists(string name) => _dbContext.ExistsAsync(name);
}