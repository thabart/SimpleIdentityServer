namespace SimpleIdentityServer.UserManagement
{
    public class UserManagementAuthenticationOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AuthorizationWellKnownConfiguration { get; set; }
    }

    public class UserManagementOptions
    {
        public UserManagementOptions()
        {
            CreateScimResourceWhenAccountIsAdded = false;
            AuthenticationOptions = new UserManagementAuthenticationOptions();
        }

        public bool CreateScimResourceWhenAccountIsAdded { get; set; }
        public string ScimBaseUrl { get; set; }
        public UserManagementAuthenticationOptions AuthenticationOptions { get; set; }
    }
}
