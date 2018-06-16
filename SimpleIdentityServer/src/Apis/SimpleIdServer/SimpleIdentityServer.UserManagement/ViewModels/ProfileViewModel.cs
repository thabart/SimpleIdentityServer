using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class IdentityProviderViewModel
    {
        public IdentityProviderViewModel(string displayName)
        {
            DisplayName = displayName;
        }

        public IdentityProviderViewModel(string displayName, string externalSubject) : this(displayName)
        {
            ExternalSubject = externalSubject;
        }

        public string DisplayName { get; set; }
        public string ExternalSubject { get; set; }
    }

    public class ProfileViewModel
    {
        public ProfileViewModel()
        {
            LinkedIdentityProviders = new List<IdentityProviderViewModel>();
            UnlinkedIdentityProviders = new List<IdentityProviderViewModel>();
        }

        public ICollection<IdentityProviderViewModel> LinkedIdentityProviders { get; set; }
        public ICollection<IdentityProviderViewModel> UnlinkedIdentityProviders { get; set; }
    }
}
