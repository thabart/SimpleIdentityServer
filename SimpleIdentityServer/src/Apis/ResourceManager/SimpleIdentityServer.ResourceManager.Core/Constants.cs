namespace SimpleIdentityServer.ResourceManager.Core
{
    public static class Constants
    {
        public const string MANAGER_SCOPE = "manager";

        public static class ErrorCodes
        {
            public const string InternalError = "internal_error";
        }

        public static class Errors
        {
            public const string ErrParamNotValidInt = "the parameter {0} is not a valid integer";
            public const string ErrParamNotSpecified = "the parameter {0} is not specified";
            public const string ErrInsertAsset = "an error occured while trying to insert the asset(s)";
            public const string ErrDuplicateAsset = "an error occured while trying to duplicate the asset(s)";
            public const string ErrUpdateAsset = "an error occured while trying to update the asset";
            public const string ErrPasteAsset = "an error occured while trying to paste the asset(s)";
            public const string ErrCutAsset = "an error occured while trying to cut the asset(s)";
            public const string ErrRemoveAssets = "an error occured while trying to remove the asset(s)";
            public const string ErrTargetsNotFound = "some targets don't exist";
            public const string ErrRemoveEndpoint = "an error occured while trying to remove the endpoint(s)";
            public const string ErrNoEndpoint = "no endpoint";
            public const string ErrAuthNotConfigured = "authorization server is not configured";
            public const string ErrManagerApiNotConfigured = "manager API is not configured";
            public const string ErrDeleteClient = "an error occured while trying to remove the client(s)";
            public const string ErrSearchClients = "an error occured while trying to search the client(s)";
            public const string ErrInsertClient = "an error occured while trying to insert the client";
            public const string ErrUpdateClient = "an error occured while trying to update the client";
            public const string ErrDeleteRo = "an error occured while trying to remove the resource owner(s)";
            public const string ErrSearchRos = "an error occured while trying to search the resource owner(s)";
        }
    }
}
