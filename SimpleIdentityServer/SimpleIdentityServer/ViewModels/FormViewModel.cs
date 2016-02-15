namespace SimpleIdentityServer.Api.ViewModels
{
    public class FormViewModel
    {
        public string IdToken { get; set; }

        public string AccessToken { get; set; }

        public string AuthorizationCode { get; set; }

        public string State { get; set; }

        public string RedirectUri { get; set; }
    }
}