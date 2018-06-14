namespace SimpleIdentityServer.UserManagement.ViewModels
{
    public class CallbackViewModel
    {
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public string State { get; set; }
    }
}
