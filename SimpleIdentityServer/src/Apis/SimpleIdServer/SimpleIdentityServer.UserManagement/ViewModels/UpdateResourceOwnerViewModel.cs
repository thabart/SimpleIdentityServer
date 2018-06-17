using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class UpdateResourceOwnerViewModel
    {
        public UpdateResourceOwnerViewModel(string login, Dictionary<string, string> editableClaims, Dictionary<string, string> notEditableClaims, bool isLocalAccount)
        {
            IsLocalAccount = isLocalAccount;
            Credentials = new UpdateResourceOwnerCredentialsViewModel
            {
                Login = login
            };
            EditableClaims = editableClaims;
            NotEditableClaims = notEditableClaims;
        }

        public bool IsLocalAccount { get; set; }
        public UpdateResourceOwnerCredentialsViewModel Credentials { get; set; }
        public Dictionary<string, string> EditableClaims { get; set; }
        public Dictionary<string, string> NotEditableClaims { get; set; }
    }
}
