using System.ComponentModel.DataAnnotations;

namespace SimpleIdentityServer.Authenticate.SMS.ViewModels
{
    public class LocalAuthenticationViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
