using Microsoft.EntityFrameworkCore;
using Thoth.Core.Models.Entities;

namespace Thoth.SQLServer;

public static class Mappings
{
    public static void MapThoth(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FeatureManager>(entity =>
        {
            entity.HasKey(p => p.Name);

            entity.Property(p => p.Name).HasMaxLength(255).IsRequired();
            entity.Property(p => p.Type).IsRequired();
            entity.Property(p => p.SubType).IsRequired(false);
            entity.Property(p => p.Enabled).IsRequired();
            entity.Property(p => p.Value).IsRequired(false);
            entity.Property(p => p.Extras).IsRequired(false);
            entity.Property(p => p.Description).IsRequired(false);
            entity.Property(p => p.CreatedAt).IsRequired().HasDefaultValueSql("getdate()");
            entity.Property(p => p.UpdatedAt).IsRequired(false);
            entity.Property(p => p.DeletedAt).IsRequired(false);

            entity.ToTable(nameof(FeatureManager), "thoth", b => b.IsTemporal());
        });
    }
}