namespace SimpleIdentityServer.Shell.ViewModels
{
    public sealed class UiModuleViewModel
    {
        public string Title { get; set; }
        public string RelativeUrl { get; set; }
        public string Picture { get; set; }
        public bool? IsAuthenticated { get; set; }
    }
}
