using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using Prism.Modularity;
using Prism.Unity;
using SimpleIdentityServer.Rfid.Client.Common;
using SimpleIdentityServer.Rfid.Client.Home;
using SimpleIdentityServer.Rfid.Common;
using System.Windows;

namespace SimpleIdentityServer.Rfid.Client
{
    public class Bootstrapper : UnityBootstrapper
    {
        private IEventAggregator _eventAggregator;

        protected override DependencyObject CreateShell()
        {
            return ServiceLocator.Current.GetInstance<Shell>();
        }
                                                                                                                 
        protected override void InitializeShell()
        {
            base.InitializeShell();
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            var cardListener = new CardListener();
            cardListener.CardReceived += CardReceived;
            cardListener.Start();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
            
        }

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();
            ModuleCatalog moduleCatalog = (ModuleCatalog)ModuleCatalog;
            moduleCatalog.AddModule(typeof(RfidHomeModule));
        }

        private void CardReceived(object sender, CardReceivedArgs e)
        {
            var evt = _eventAggregator.GetEvent<CardReceivedEvent>();
            evt.Publish(new CardInformation
            {
                CardNumber = e.CardNumber,
                IdentityToken = e.IdentityToken
            });
        }
    }
}
