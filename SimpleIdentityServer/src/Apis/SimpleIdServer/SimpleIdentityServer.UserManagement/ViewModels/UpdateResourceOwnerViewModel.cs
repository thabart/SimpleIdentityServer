using SimpleIdentityServer.Core.Common.Models;
using System.ComponentModel.DataAnnotations;

namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class UpdateResourceOwnerViewModel
    {
        public bool IsLocalAccount { get; set; }
        public string Login { get; set; }
        [Required]
        public string Name { get; set; }
        public string Password { get; set; }
        [Required]
        public string NewPassword { get; set; }        
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public TwoFactorAuthentications TwoAuthenticationFactor { get; set; }
    }
}