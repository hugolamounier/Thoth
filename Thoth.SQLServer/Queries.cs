namespace Thoth.SQLServer;

public static class Queries
{
    public const string IsEnabledQuery = """
        SELECT Value FROM {0}.FeatureFlag WHERE Name = @Name;
    """;

    public const string AddFeatureFlagQuery = """
        INSERT INTO {0}.FeatureFlag
            (Name, Type, Value, FilterValue, CreatedAt, UpdatedAt)
        VALUES
            (@Name, @Type, @Value, @FilterValue, @CreatedAt, @UpdatedAt);
    """;

    public const string UpdateFeatureFlag = """
        UPDATE {0}.FeatureFlag
        SET
            Value = @Value, FilterValue = @FilterValue, UpdatedAt = @UpdatedAt
        WHERE
            Name = @Name;
    """;

    public const string DeleteFeatureFlagQuery = """
        DELETE FROM {0}.FeatureFlag WHERE Name = @Name;
    """;

    public const string CreateSchemaIfNotExistsQuery = """
        IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}')
        BEGIN
            EXEC('CREATE SCHEMA [{0}] AUTHORIZATION [dbo]')
        END
    """;

   public const string CreateFeatureFlagTableQuery = """
        CREATE TABLE IF NOT EXISTS
        {0}.FeatureFlag (
            Name VARCHAR(100) NOT NULL PRIMARY KEY,
            Type TINYINT NOT NULL,
            Value BIT NOT NULL,
            FilterValue VARCHAR(100) NULL,
            CreatedAt DATETIME NOT NULL,
            UpdatedAt DATETIME NULL
        );
    """;

}