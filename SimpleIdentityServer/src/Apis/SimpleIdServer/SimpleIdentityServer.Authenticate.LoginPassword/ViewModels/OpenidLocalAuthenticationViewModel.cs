using SimpleIdentityServer.Authenticate.Basic.ViewModels;

namespace SimpleIdentityServer.Authenticate.LoginPassword.ViewModels
{
    public class OpenidLocalAuthenticationViewModel : AuthorizeOpenIdViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
