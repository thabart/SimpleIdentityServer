using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class UpdateResourceOwnerViewModel
    {
        public UpdateResourceOwnerViewModel(string login, Dictionary<string, string> claims, bool isLocalAccount)
        {
            IsLocalAccount = isLocalAccount;
            Credentials = new UpdateResourceOwnerCredentialsViewModel
            {
                Login = login
            };
            Claims = new UpdateResourceOwnerClaimsViewModel
            {
                Claims = claims
            };
        }

        public bool IsLocalAccount { get; set; }
        public UpdateResourceOwnerCredentialsViewModel Credentials { get; set; }
        public UpdateResourceOwnerClaimsViewModel Claims { get; set; }
    }
}
