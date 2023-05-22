using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.MongoDb;

public class ThothMongoDbProvider : IDatabase
{
    private readonly IMongoCollection<FeatureManager> _mongoCollection;
    private readonly IMongoCollection<FeatureManager> _mongoDeletedCollection;
    private readonly IMongoDatabase _mongoDatabase;

    public ThothMongoDbProvider(IMongoClient mongoClient)
    {
        var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => type == typeof(FeatureManager));

        _mongoDatabase = mongoClient.GetDatabase(ThothMongoDbOptions.DatabaseName);

        _mongoCollection = _mongoDatabase.GetCollection<FeatureManager>(ThothMongoDbOptions.CollectionName);

        _mongoDeletedCollection = _mongoDatabase.GetCollection<FeatureManager>($"{ThothMongoDbOptions.CollectionName}_Deleted");

        Init();
    }

    public async Task<FeatureManager> GetAsync(string featureName)
    {
        return await _mongoCollection.Find(c => c.Name == featureName).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FeatureManager>> GetAllAsync()
    {
        var features = await _mongoCollection
            .Find(_ => true)
            .ToListAsync();

        var orderedFeatures = features.Select(x =>
        {
            x.Histories = x.Histories.OrderByDescending(f => f.PeriodEnd).ToList();
            return x;
        });

        return orderedFeatures.OrderByDescending(f => f.CreatedAt);
    }

    public async Task<IEnumerable<FeatureManager>> GetAllDeletedAsync()
    {
        var deletedFeatures = await _mongoDeletedCollection.Find(_ => true).ToListAsync();
        var orderedFeatures = deletedFeatures.Select(x =>
        {
            x.Histories = x.Histories.OrderByDescending(f => f.PeriodEnd).ToList();
            return x;
        });

        return orderedFeatures.OrderByDescending(f => f.DeletedAt);
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

        featureManager.Histories = feature.Histories;
        featureManager.Histories.Add(featureHistory);
        
        await _mongoCollection.ReplaceOneAsync(f => f.Name == featureManager.Name, featureManager);
        return true;
    }

    public async Task<bool> DeleteAsync(string featureName, string auditExtras = "")
    {
        using var session = await _mongoDatabase.Client.StartSessionAsync();
        try
        {
            session.StartTransaction();
            var feature = await GetAsync(featureName);
            feature.DeletedAt = DateTime.UtcNow;
            feature.Extras = auditExtras;

            if(ThothMongoDbOptions.DeletedFeaturesTtl is not null)
                feature.ExpiresAt = feature.DeletedAt + ThothMongoDbOptions.DeletedFeaturesTtl;

            await _mongoCollection.DeleteOneAsync(f => f.Name == featureName);
            await _mongoDeletedCollection.InsertOneAsync(feature);

            await session.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await session.AbortTransactionAsync();
            return false;
        }
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

        _mongoDeletedCollection.Indexes.CreateOne(new CreateIndexModel<FeatureManager>(
            Builders<FeatureManager>.IndexKeys.Ascending(i => i.Name)));
        
        _mongoDeletedCollection.Indexes.CreateOne(new CreateIndexModel<FeatureManager>(
            Builders<FeatureManager>.IndexKeys.Ascending(i => i.ExpiresAt),
            new CreateIndexOptions { ExpireAfter = new TimeSpan(0) }));
    }
}