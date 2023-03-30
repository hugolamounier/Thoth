namespace Thoth.Models;

public static class Messages
{
    public const string ERROR_FEATURE_FLAG_ALREADY_EXISTS = "The feature flag '{0}' already exists, please choose a different name";
    public const string ERROR_FEATURE_FLAG_NOT_EXISTS = "The feature flag '{0}' doesn't exists, please verify the name";
    public const string ERROR_WHILE_ADDIND_FEATURE_FLAG = "An error ocurred while adding the feature flag";
    public const string ERROR_BOOLEAN_FEATURE_FLAGS_CANT_HAVE_FILTER_VALUE = "A boolean feature flag must have FilterValue null";
    public const string ERROR_SQL_SERVER_IS_REQUIRED = "When set to use SQL Server, it is required to set SqlServerConnectionString option";

    public const string INFO_ADDED_FEATURE_FLAG = "The feature flag '{0}' was successfully added with the initial value {1} and filterValue {2}";
    public const string INFO_UPDATED_FEATURE_FLAG = "The feature flag '{0}' was successfully updated with the value {1} and filterValue {2}";

}