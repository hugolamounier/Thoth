using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Thoth.Core.Interfaces;
using Thoth.Core.Models.Entities;

namespace Thoth.SQLServer;

public sealed class ThothSqlServerProvider : DbContext, IDatabase
{
    private readonly string _connectionString;
    private const string SchemaName = "thoth";

    public ThothSqlServerProvider(DbContextOptions options) : base(options){}
    public ThothSqlServerProvider(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(connectionString);

        _connectionString = connectionString;
        Init();
    }

    public Task<FeatureManager?> GetAsync(string featureName) =>
        Features.FirstOrDefaultAsync(x => x.Name == featureName);

    public Task<List<FeatureManager>> GetAllAsync() =>
        Features.AsNoTracking().ToListAsync();

    public async Task<bool> AddAsync(FeatureManager featureManager)
    {
        await Features.AddAsync(featureManager);
        return await SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(FeatureManager featureManager)
    {
        var feature = await GetAsync(featureManager.Name);

        if (feature is null)
            return false;

        feature.Description = featureManager.Description;
        feature.Value = featureManager.Value;
        feature.Enabled = featureManager.Enabled;

        return await SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(string featureName)
    {
        var featureToDelete = await GetAsync(featureName);

        if (featureToDelete is null)
            return false;

        Features.Remove(featureToDelete);
        return await SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(string featureName) =>
        await GetAsync(featureName) is not null;

    private DbSet<FeatureManager> Features { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(_connectionString, x =>
        {
            x.MigrationsHistoryTable("__EFMigrationsHistory", SchemaName);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<FeatureManager>(entity =>
        {
            entity.HasKey(p => p.Name);

            entity.Property(p => p.Name).HasMaxLength(100);
            entity.Property(p => p.Type);
            entity.Property(p => p.SubType).IsRequired(false);
            entity.Property(p => p.Enabled);
            entity.Property(p => p.Description).IsRequired(false).HasMaxLength(200);
            entity.Property(p => p.Value).IsRequired(false);
            entity.Property(p => p.CreatedAt);
            entity.Property(p => p.UpdatedAt).IsRequired(false);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void Init()
    {
        Database.Migrate();
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x is {Entity: IThothFeatureEntity, State: EntityState.Added or EntityState.Modified});

        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
                ((IThothFeatureEntity)entity.Entity).CreatedAt = DateTime.UtcNow;

            ((IThothFeatureEntity)entity.Entity).UpdatedAt = DateTime.UtcNow;
        }
    }
}