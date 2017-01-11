using System.Windows;
using Prism.Unity;
using Microsoft.Practices.ServiceLocation;
using Prism.Modularity;
using SimpleIdentityServer.Rfid.Home;

namespace SimpleIdentityServer.Rfid.Client
{
    public class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return ServiceLocator.Current.GetInstance<Shell>();
        }
                                                                                                                 
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();
            ModuleCatalog moduleCatalog = (ModuleCatalog)ModuleCatalog;
            moduleCatalog.AddModule(typeof(RfidHomeModule));
        }
    }
}
