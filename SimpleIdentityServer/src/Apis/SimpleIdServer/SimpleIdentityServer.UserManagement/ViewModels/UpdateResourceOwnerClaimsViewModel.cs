using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class UpdateResourceOwnerClaimsViewModel
    {
        public Dictionary<string, string> Claims { get; set; }
    }
}
