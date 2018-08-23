namespace SimpleIdentityServer.Scim.Mapping.Ad
{
    internal static class ErrorDescriptions
    {
        public const string NoRequest = "no request passed in the body";
        public const string MappingAlreadyAssigned = "a mapping has already been assigned";
        public const string CannotInsertMapping = "an error occured while trying to insert the mapping";
        public const string MissingParameter = "the parameter {0} is missing";
        public const string MappingDoesntExist = "the mapping doesn't exist";
        public const string CannotDeleteMapping = "an error occcured while trying to remove the mapping";
    }
}
