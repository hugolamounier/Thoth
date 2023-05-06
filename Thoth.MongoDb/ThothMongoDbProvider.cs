using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.MongoDb;

public class ThothMongoDbProvider : IDatabase
{
    private readonly IMongoCollection<FeatureManager> _mongoCollection;

    public ThothMongoDbProvider(IMongoClient mongoClient)
    {
        var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => type == typeof(FeatureManager));
        
        _mongoCollection = mongoClient.GetDatabase(ThothMongoDbOptions.DatabaseName)
            .GetCollection<FeatureManager>(ThothMongoDbOptions.CollectionName);

        Init();
    }

    public async Task<FeatureManager> GetAsync(string featureName)
    {
        return await _mongoCollection.Find(c => c.Name == featureName &&
                                                c.DeletedAt == null).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FeatureManager>> GetAllAsync()
    {
        return await _mongoCollection.Find(f => f.DeletedAt == null).ToListAsync();
    }

    public async Task<bool> AddAsync(FeatureManager featureManager)
    {
        featureManager.CreatedAt = DateTime.UtcNow;
        await _mongoCollection.InsertOneAsync(featureManager);
        return true;
    }

    public async Task<bool> UpdateAsync(FeatureManager featureManager)
    {
        var feature = await GetAsync(featureManager.Name);
        var featureHistory = new FeatureManagerHistory(feature);
        
        if(ThothMongoDbOptions.FeatureHistoryTtl is not null)
            featureHistory.ExpiresAt = DateTime.UtcNow + ThothMongoDbOptions.FeatureHistoryTtl;
        
        featureManager.Histories.Add(featureHistory);
        
        await _mongoCollection.ReplaceOneAsync(f => f.Name == featureManager.Name &&
                                                    f.DeletedAt == null, featureManager);
        return true;
    }

    public async Task<bool> DeleteAsync(string featureName)
    {
        var feature = await GetAsync(featureName);
        feature.DeletedAt = DateTime.UtcNow;
        
        if(ThothMongoDbOptions.DeletedFeaturesTtl is not null)
            feature.ExpiresAt = feature.DeletedAt + ThothMongoDbOptions.DeletedFeaturesTtl;

        await _mongoCollection.ReplaceOneAsync(f => f.Name == featureName, feature);
        return true;
    }

    public async Task<bool> ExistsAsync(string featureName)
    {
        return await GetAsync(featureName) is not null;
    }

    private void Init()
    {
        _mongoCollection.Indexes.CreateOne(new CreateIndexModel<FeatureManager>(
            Builders<FeatureManager>.IndexKeys.Ascending(i => i.Name),
            new CreateIndexOptions { Unique = true }));
        
        _mongoCollection.Indexes.CreateOne(new CreateIndexModel<FeatureManager>(
            Builders<FeatureManager>.IndexKeys.Ascending(i => i.ExpiresAt),
            new CreateIndexOptions { ExpireAfter = new TimeSpan(0) }));
    }
}