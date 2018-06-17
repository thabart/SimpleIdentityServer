namespace SimpleIdentityServer.UserManagement
{
    public class ScimOptions
    {
        public string ScimBaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationWellKnownConfiguration { get; set; }
    }

    public class UserManagementOptions
    {
        public UserManagementOptions()
        {
            CreateScimResourceWhenAccountIsAdded = false;
        }

        public bool CreateScimResourceWhenAccountIsAdded { get; set; }
        public ScimOptions Scim { get; set; }
    }
}
