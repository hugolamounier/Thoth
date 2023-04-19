using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.MongoDb;

public class ThothMongoDbProvider: IDatabase
{
    private readonly IMongoCollection<FeatureManager> _mongoCollection;

    public ThothMongoDbProvider(IMongoClient mongoClient, string databaseName, string collectionName = "thoth")
    {
        var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => type == typeof(FeatureManager));

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
        await _mongoCollection.InsertOneAsync(featureManager);
        return true;
    }

    public async Task<bool> UpdateAsync(FeatureManager featureManager)
    {
        await _mongoCollection.ReplaceOneAsync(f => f.Name == featureManager.Name, featureManager);
        return true;
    }

    public async Task<bool> DeleteAsync(string featureName)
    {
        await _mongoCollection.DeleteOneAsync(f => f.Name == featureName);
        return true;
    }

    public async Task<bool> ExistsAsync(string featureName)
    {
        return await GetAsync(featureName) is not null;
    }

    private void Init()
    {
        var nameIndex = Builders<FeatureManager>.IndexKeys.Ascending(i => i.Name);
        var nameIndexOptions = new CreateIndexOptions {Unique = true};
        _mongoCollection.Indexes.CreateOne(new CreateIndexModel<FeatureManager>(nameIndex, nameIndexOptions));
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // No cleanup needed
    }
}