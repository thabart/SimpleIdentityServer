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
