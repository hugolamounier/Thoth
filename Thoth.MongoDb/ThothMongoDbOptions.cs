using System;

namespace Thoth.MongoDb;

internal static class ThothMongoDbOptions
{
    private static string _databaseName;

    /// <summary>
    ///     Defines the collection name to be used by Thoth
    /// </summary>
    public static string CollectionName { get; set; }

    /// <summary>
    ///     Defines the TTL of deleted features
    /// </summary>
    public static TimeSpan? DeletedFeaturesTtl { get; set; }

    public static string DatabaseName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_databaseName))
                throw new InvalidOperationException("No MongoDb database name provided. " +
                                                    "Make sure you are injecting 'UseMongoDb' to Thoth options");

            return _databaseName;
        }
        set => _databaseName = value;
    }
}