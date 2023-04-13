using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Thoth.SQLServer;


/// <summary>
///     Only used to generate migrations
/// </summary>
[ExcludeFromCodeCoverage]
public class DbContextFactory: IDesignTimeDbContextFactory<ThothSqlServerProvider>
{
    public ThothSqlServerProvider CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ThothSqlServerProvider>();
        optionsBuilder.UseSqlServer(x =>
        {
            x.MigrationsHistoryTable("__EFMigrationsHistory", "thoth");
        });

        return new ThothSqlServerProvider(optionsBuilder.Options);
    }
}