using RfidValidator.Rfid;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RfidValidator
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            this.Content.Navigate(typeof(ValidateTab));
            var cardListener = new CardListener();
            cardListener.CardReceived += CardReceived;
        }

        private void CardReceived(object sender, CardReceivedArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void NavigateToValidate(object sender, RoutedEventArgs e)
        {
            this.Content.Navigate(typeof(ValidateTab));
        }

        private void NavigateToAccount(object sender, RoutedEventArgs e)
        {
            this.Content.Navigate(typeof(AccountTab));
        }
    }
}
