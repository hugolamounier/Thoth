namespace Thoth.Core.Models;

public static class Messages
{
    public const string ERROR_FEATURE_FLAG_ALREADY_EXISTS = "The feature flag '{0}' already exists, please choose a different name.";
    public const string ERROR_FEATURE_FLAG_NOT_EXISTS = "The feature flag '{0}' doesn't exists, please verify the name.";
    public const string ERROR_WHILE_ADDIND_FEATURE_FLAG = "An error ocurred while adding the feature flag.";
    public const string ERROR_WHILE_UPDATING_FEATURE_FLAG = "An error ocurred while updating the feature flag: '{0}'.";
    public const string ERROR_WHILE_DELETING_FEATURE_FLAG = "An error ocurred while deleting the feature flag: '{0}'.";
    public const string ERROR_BOOLEAN_FEATURE_FLAGS_CANT_HAVE_VALUE = "A boolean feature flag must have Value null.";
    public const string ERROR_DATABASE_PROVIDER = "The 'DatabaseProvider' options must be set.";
    public const string ERROR_CAN_NOT_USE_THOTH_DASHBOARD = "The option EnableThothApi needs to be set 'true' to use Thoth Dashboard.";
    public const string ERROR_CAN_NOT_GET_DISABLED_FEATURE = "Disabled features cannot be retrived.";
    public const string ERROR_WRONG_FEATURE_TYPE = "The feature '{0}' is not of Type '{1}'. Therefore, you cannot get its Value";

    public const string INFO_NON_EXISTENT_FLAG_REQUESTED =
        "Non-existent feature flag '{0}' requested, returning false (Option 'ShouldReturnFalseWhenNotExists' enabled)";
    public const string INFO_ADDED_FEATURE_FLAG = "The feature flag '{0}' was successfully added with the initial state '{1}' and Value '{2}'.";
    public const string INFO_UPDATED_FEATURE_FLAG = "The feature flag '{0}' was successfully updated with the state {1} and Value {2}.";
    public const string INFO_DELETED_FEATURE_FLAG = "The feature flag '{0}' was successfully deleted.";
    public const string INFO_ACTION_MADE_BY_USER_WITH_CLAIMS = "Action performed by the user with de following set claims: {0}";

    public const string VALIDATION_INVALID_FIELD= "The field '{0}' is required.";
}