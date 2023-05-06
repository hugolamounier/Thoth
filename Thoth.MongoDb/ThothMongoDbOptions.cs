using System;

namespace Thoth.MongoDb;

internal static class ThothMongoDbOptions
{
    private static string _databaseName;

    public static string CollectionName { get; set; }
    public static TimeSpan? DeletedFeaturesTtl { get; set; }
    public static TimeSpan? FeatureHistoryTtl { get; set; }

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