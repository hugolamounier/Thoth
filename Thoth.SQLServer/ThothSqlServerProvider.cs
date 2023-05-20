using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
        return (await _featureManagers.ToListAsync()).AsEnumerable();
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
            var featureToRemove = await GetAsync(featureName);

            _featureManagers.Remove(featureToRemove);

            await _dbContext.SaveChangesAsync();

            await _dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE thoth.FeatureManager SET (SYSTEM_VERSIONING = OFF);");

            if (!string.IsNullOrEmpty(auditExtras))
            {
                await _dbContext.Database.ExecuteSqlRawAsync(@"    
                    UPDATE 
                        thoth.FeatureManagerHistory 
                    SET 
                        DeletedAt = {0}, Extras = {1}
                    WHERE 
                        Name = {2} AND 
                        PeriodEnd = (SELECT TOP 1 PeriodEnd FROM thoth.FeatureManagerHistory WHERE Name = {2} ORDER BY PeriodEnd DESC)",
                        DateTime.UtcNow, auditExtras, featureName);
            }

            await _dbContext.Database.ExecuteSqlRawAsync(@"
                ALTER TABLE 
                    thoth.FeatureManager
                SET
                    (
                        SYSTEM_VERSIONING = ON
                        (HISTORY_TABLE = thoth.FeatureManagerHistory)
                    );
                ");

            return true;
        }
        catch
        {
            var restoreFeature = await _featureManagers
                .TemporalAll()
                .Where(x => x.Name == featureName)
                .OrderBy(x => EF.Property<DateTime>(x, "PeriodEnd"))
                .LastAsync();
            await _featureManagers.AddAsync(restoreFeature);
            await _dbContext.SaveChangesAsync();
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string featureName)
    {
        return await _featureManagers.FirstOrDefaultAsync(x => x.Name == featureName) is not null;
    }
}