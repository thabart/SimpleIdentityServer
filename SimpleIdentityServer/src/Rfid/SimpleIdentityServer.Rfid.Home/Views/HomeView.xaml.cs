using Prism.Events;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Rfid.Client.Common;
using System.Windows.Controls;

namespace SimpleIdentityServer.Rfid.Client.Home.Views
{
    public partial class HomeView : UserControl
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IJwsParser _jwsParser;

        public HomeView(IEventAggregator eventAggregator, IJwsParser jwsParser)
        {
            _eventAggregator = eventAggregator;
            _jwsParser = jwsParser;
            InitializeComponent();
        }

        private void HomeViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var evt = _eventAggregator.GetEvent<CardReceivedEvent>();
            evt.Subscribe(CardReceived);
        }

        private void CardReceived(CardInformation cardInformation)
        {
            string s = "";
        }
    }
}
