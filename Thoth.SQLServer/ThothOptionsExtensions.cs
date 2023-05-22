using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.SQLServer;

public static class ThothOptionsExtensions
{
    /// <summary>
    ///     Register Thoth to use EF Core as its database provider
    /// </summary>
    /// <param name="thothOptions"></param>
    /// <typeparam name="TContext">Type of the DbContext</typeparam>
    public static void UseEntityFramework<TContext>(this ThothOptions thothOptions)
        where TContext : DbContext
    {
        static void ThothDatabaseSetup(IServiceCollection services)
        {
            services.AddScoped<IDatabase, ThothSqlServerProvider<TContext>>();
        }

        thothOptions.Extensions.Add(ThothDatabaseSetup);
    }
}