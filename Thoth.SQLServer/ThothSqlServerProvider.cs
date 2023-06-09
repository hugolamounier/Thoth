using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.SQLServer;

public class ThothSqlServerProvider<TContext> : IDatabase
    where TContext : DbContext
{
    private readonly TContext _dbContext;
    private readonly DbSet<FeatureManager> _featureManagers;

    public ThothSqlServerProvider(TContext dbContext)
    {
        _dbContext = dbContext;
        _featureManagers = dbContext.Set<FeatureManager>();
    }

    public async Task<FeatureManager> GetAsync(string featureName)
    {
        return await _featureManagers.FirstAsync(x => x.Name == featureName);
    }

    public async Task<IEnumerable<FeatureManager>> GetAllAsync()
    {
        var features = await _featureManagers.TemporalAll()
            .OrderByDescending(c => EF.Property<DateTime>(c, "PeriodEnd"))
            .Select(x => new
            {
                Feature = x,
                ValidFrom = EF.Property<DateTime>(x, "PeriodStart"),
                ValidTo = EF.Property<DateTime>(x, "PeriodEnd")
            })
            .ToListAsync();

        var currentFeatures = features
            .AsParallel()
            .Where(x => x.ValidTo == DateTime.MaxValue)
            .Select(x => x.Feature).ToList();

        var resultFeatures = currentFeatures.AsParallel().Select(x =>
        {
            x.Histories = features
                .AsParallel()
                .Where(y => y.ValidTo < DateTime.MaxValue && y.Feature.Name == x.Name)
                .Select(y => new FeatureManagerHistory(y.Feature, y.ValidFrom, y.ValidTo))
                .ToList();

            return x;
        }).OrderByDescending(x => x.CreatedAt);

        return resultFeatures;
    }

    public async Task<IEnumerable<FeatureManager>> GetAllDeletedAsync()
    {
        var features = await _featureManagers.TemporalAll()
            .Where(c => c.DeletedAt != null)
            .OrderByDescending(c => EF.Property<DateTime>(c, "PeriodEnd"))
            .Select(x => new
            {
                Feature = x,
                ValidFrom = EF.Property<DateTime>(x, "PeriodStart"),
                ValidTo = EF.Property<DateTime>(x, "PeriodEnd")
            })
            .ToListAsync();

        var currentFeatures = features
            .AsParallel()
            .Where(x => x.ValidTo < DateTime.MaxValue)
            .Select(x => x.Feature).ToList();

        var resultFeatures = currentFeatures.AsParallel().Select(x =>
        {
            x.Histories = features
                .AsParallel()
                .Where(y => y.ValidTo < DateTime.MaxValue && y.Feature.Name == x.Name)
                .Select(y => new FeatureManagerHistory(y.Feature, y.ValidFrom, y.ValidTo))
                .ToList();

            return x;
        }).OrderByDescending(x => x.DeletedAt);

        return resultFeatures;
    }

    public async Task<bool> AddAsync(FeatureManager featureManager)
    {
        featureManager.CreatedAt = DateTime.UtcNow;
        await _featureManagers.AddAsync(featureManager);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(FeatureManager featureManager)
    {
        var featureToUpdate = await GetAsync(featureManager.Name);

        featureToUpdate.Enabled = featureManager.Enabled;
        featureToUpdate.Value = featureManager.Value;
        featureToUpdate.Description = featureManager.Description;
        featureToUpdate.Extras = featureManager.Extras;
        featureToUpdate.UpdatedAt = DateTime.UtcNow;

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(string featureName, string auditExtras = "")
    {
        try
        {
            await _dbContext.Database.BeginTransactionAsync();
            var featureToRemove = await GetAsync(featureName);

            _featureManagers.Remove(featureToRemove);

            await _dbContext.SaveChangesAsync();

            await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE thoth.FeatureManager SET (SYSTEM_VERSIONING = OFF);");

            await _dbContext.Database.ExecuteSqlRawAsync(@"    
                    UPDATE 
                        thoth.FeatureManagerHistory 
                    SET 
                        DeletedAt = {0}, Extras = {1}
                    WHERE 
                        Name = {2} AND 
                        PeriodEnd = (SELECT TOP 1 PeriodEnd FROM thoth.FeatureManagerHistory WHERE Name = {2} ORDER BY PeriodEnd DESC)",
                DateTime.UtcNow, auditExtras, featureName);

            await _dbContext.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE 
                    thoth.FeatureManager
                SET
                    (
                        SYSTEM_VERSIONING = ON
                        (HISTORY_TABLE = thoth.FeatureManagerHistory)
                    );
                ");

            await _dbContext.Database.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _dbContext.Database.RollbackTransactionAsync();
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string featureName)
    {
        return await _featureManagers.FirstOrDefaultAsync(x => x.Name == featureName) is not null;
    }
}