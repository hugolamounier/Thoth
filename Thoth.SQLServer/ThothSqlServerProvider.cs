using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.SQLServer;

public class ThothSqlServerProvider<TContext>: IDatabase where TContext : DbContext
{
    private readonly DbSet<FeatureManager> _featureManagers;
    private readonly TContext _dbContext;

    public ThothSqlServerProvider(TContext dbContext)
    {
        _dbContext = dbContext;
        _featureManagers = dbContext.Set<FeatureManager>();
    }

    public Task<FeatureManager> GetAsync(string featureName)
    {
        return _featureManagers.FirstAsync(x => x.Name == featureName);
    }

    public Task<IEnumerable<FeatureManager>> GetAllAsync()
    {
        return Task.FromResult(_featureManagers.AsEnumerable());
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
        featureToUpdate.UpdatedAt = DateTime.UtcNow;

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(string featureName)
    {
        var featureToRemove = await GetAsync(featureName);
        _featureManagers.Remove(featureToRemove);

        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(string featureName)
    {
        return await _featureManagers.FirstOrDefaultAsync(x => x.Name == featureName) is not null;
    }
}