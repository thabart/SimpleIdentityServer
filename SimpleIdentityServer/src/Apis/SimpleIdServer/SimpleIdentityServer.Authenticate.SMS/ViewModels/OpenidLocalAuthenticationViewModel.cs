using SimpleIdentityServer.Authenticate.Basic.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace SimpleIdentityServer.Authenticate.SMS.ViewModels
{
    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
