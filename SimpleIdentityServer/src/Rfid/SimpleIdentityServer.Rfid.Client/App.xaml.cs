using System.Windows;

namespace SimpleIdentityServer.Rfid.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            (new Bootstrapper()).Run();
        }
    }
}
