using Microsoft.EntityFrameworkCore;
using Thoth.SQLServer;

namespace Thoth.Sample.Contexts;

public class SqlContext : DbContext
{
    public SqlContext(DbContextOptions options) :base(options){}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.MapThoth();
    }
}