using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Thoth.Core.Models.Entities;

namespace Thoth.MongoDb;

public class ThothMongoHistoryPurger: BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ThothMongoHistoryPurger(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromHours(12));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var dateTimeNow = DateTime.UtcNow;
            var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
            var mongoCollection = mongoClient.GetDatabase(ThothMongoDbOptions.DatabaseName)
                .GetCollection<FeatureManager>(ThothMongoDbOptions.CollectionName);
            
            var historiesToRemove = await mongoCollection
                .Find(f => f.Histories.Any(h => h.ExpiresAt <= dateTimeNow))
                .ToListAsync(cancellationToken: stoppingToken);

            if (!historiesToRemove.Any())
                continue;
            
            var bulkOperation = new List<ReplaceOneModel<FeatureManager>>();
            historiesToRemove.ForEach(feature =>
            {
                feature.Histories.RemoveAll(h => h.ExpiresAt <= dateTimeNow);
                bulkOperation.Add(new ReplaceOneModel<FeatureManager>(
                    new ExpressionFilterDefinition<FeatureManager>(doc => doc.Name == feature.Name), feature)
                {
                    IsUpsert = true
                });
            });

            await mongoCollection.BulkWriteAsync(bulkOperation, cancellationToken: stoppingToken);
        }
    }
}