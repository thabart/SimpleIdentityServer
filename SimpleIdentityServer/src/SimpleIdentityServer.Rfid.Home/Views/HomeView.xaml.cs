using Prism.Events;
using SimpleIdentityServer.Rfid.Client.Common;
using System.Windows.Controls;

namespace SimpleIdentityServer.Rfid.Client.Home.Views
{
    public partial class HomeView : UserControl
    {
        private readonly IEventAggregator _eventAggregator;

        public HomeView(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitializeComponent();
        }

        private void HomeViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var evt = _eventAggregator.GetEvent<CardReceivedEvent>();
            evt.Subscribe(CardReceived);
        }

        private void CardReceived(CardInformation cardInformation)
        {
            // PUT THE LOGIC HERE TO VALIDATE THE CARD.

            string s = "";
        }
    }
}
