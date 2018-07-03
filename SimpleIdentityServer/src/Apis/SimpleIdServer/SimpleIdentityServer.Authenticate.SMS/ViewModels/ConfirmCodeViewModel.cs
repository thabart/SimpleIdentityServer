namespace SimpleIdentityServer.Authenticate.SMS.ViewModels
{
    public class ConfirmCodeViewModel
    {
        public string Code { get; set; }
        public string ConfirmationCode { get; set; }
        public string Action { get; set; }
    }
}
