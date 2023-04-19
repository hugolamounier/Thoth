using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.MongoDb;

public class ThothMongoDbProvider: IDatabase
{
    private readonly IMongoCollection<FeatureManager> _mongoCollection;
    private readonly ILogger<ThothMongoDbProvider> _logger;

    public ThothMongoDbProvider(IMongoClient mongoClient, string databaseName, string collectionName = "thoth", ILogger<ThothMongoDbProvider> logger = null)
    {
        _logger = logger;
        _mongoCollection = mongoClient
            .GetDatabase(databaseName)
            .GetCollection<FeatureManager>(collectionName);

        Init();
    }

    public async Task<FeatureManager> GetAsync(string featureName)
    {
        return await _mongoCollection.Find(c => c.Name == featureName).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FeatureManager>> GetAllAsync()
    {
        return await _mongoCollection.Find(_ => true).ToListAsync();
    }

    public async Task<bool> AddAsync(FeatureManager featureManager)
    {
        try
        {
            await _mongoCollection.InsertOneAsync(featureManager);
            return true;
        }
        catch(Exception e)
        {
            _logger?.Log(LogLevel.Error, e,
                "{Provider} - The feature '{FeatureName}' could not be inserted",nameof(ThothMongoDbProvider), featureManager.Name);
            return false;
        }
    }

    public async Task<bool> UpdateAsync(FeatureManager featureManager)
    {
        try
        {
            await _mongoCollection.ReplaceOneAsync(f => f.Name == featureManager.Name, featureManager);
            return true;
        }
        catch(Exception e)
        {
            _logger?.Log(LogLevel.Error, e,
                "{Provider} - The feature '{FeatureName}' could not be updated", nameof(ThothMongoDbProvider), featureManager.Name);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(string featureName)
    {
        try
        {
            await _mongoCollection.DeleteOneAsync(f => f.Name == featureName);
            return true;
        }
        catch(Exception e)
        {
            _logger?.Log(LogLevel.Error, e,
                "{Provider} - The feature '{FeatureName}' could not be deleted", nameof(ThothMongoDbProvider), featureName);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string featureName)
    {
        return await GetAsync(featureName) is not null;
    }

    private void Init()
    {
        _mongoCollection.Indexes.CreateOne(new CreateIndexModel<FeatureManager>(Builders<FeatureManager>.IndexKeys.Ascending(i => i.Name)));
    }

    public void Dispose()
    {
    }
}