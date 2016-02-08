namespace SimpleIdentityServer.Api.ViewModels
{
    public class AuthorizeViewModel
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsChecked { get; set; }
    }
}