namespace Thoth.SQLServer;

public static class Queries
{
    public const string IsEnabledQuery = @"
        SELECT Value FROM {0}.FeatureFlag WHERE Name = @Name;
    ";

    public const string GetQuery = @"
        SELECT * FROM {0}.FeatureFlag WHERE Name = @Name;
    ";

    public const string GetAllQuery = @"
        SELECT * FROM {0}.FeatureFlag;
    ";

    public const string AddFeatureFlagQuery = @"
        INSERT INTO {0}.FeatureFlag
            (Name, Type, Value, FilterValue, CreatedAt, UpdatedAt)
        VALUES
            (@Name, @Type, @Value, @FilterValue, @CreatedAt, @UpdatedAt);
    ";

    public const string UpdateFeatureFlag = @"
        UPDATE {0}.FeatureFlag
        SET
            Value = @Value, FilterValue = @FilterValue, UpdatedAt = @UpdatedAt
        WHERE
            Name = @Name;
    ";

    public const string DeleteFeatureFlagQuery = @"
        DELETE FROM {0}.FeatureFlag WHERE Name = @Name;
    ";

    public const string CreateSchemaIfNotExistsQuery = @"
        IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}')
        BEGIN
            EXEC('CREATE SCHEMA [{0}] AUTHORIZATION [dbo]')
        END
    ";

   public const string CreateFeatureFlagTableQuery = @"
        IF NOT EXISTS (SELECT * FROM sys.objects
            WHERE object_id = OBJECT_ID(N'[{0}].[FeatureFlag]') AND type in (N'U'))
        BEGIN
            CREATE TABLE [{0}].[FeatureFlag] (
                Name VARCHAR(100) NOT NULL PRIMARY KEY,
                Type TINYINT NOT NULL,
                Value BIT NOT NULL,
                FilterValue VARCHAR(100) NULL,
                CreatedAt DATETIME NOT NULL,
                UpdatedAt DATETIME NULL
            );
        END
    ";

}