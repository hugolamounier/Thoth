using Microsoft.Extensions.DependencyInjection;
using Thoth.Core.Interfaces;
using Thoth.Core.Models;

namespace Thoth.MongoDb;

public static class ThothOptionsExtensions
{
    /// <summary>
    ///     Register Thoth to use MongoDb as its database provider
    /// </summary>
    /// <param name="thothOptions"></param>
    /// <param name="databaseName">The name of the MongoDb database</param>
    /// <param name="collectionName">The name of the collection thoth is going to use</param>
    public static void UseMongoDb(this ThothOptions thothOptions, string databaseName, string collectionName = "thoth")
    {
        ThothMongoDbOptions.DatabaseName = databaseName;
        ThothMongoDbOptions.CollectionName = collectionName;
        
        static void ThothDatabaseSetup(IServiceCollection services)
        {
            services.AddScoped<IDatabase, ThothMongoDbProvider>();
        }

        thothOptions.Extensions.Add(ThothDatabaseSetup);
    }
}