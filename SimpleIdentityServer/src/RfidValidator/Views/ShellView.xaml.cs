using Windows.UI.Xaml;

namespace RfidValidator.Views
{
    public sealed partial class ShellView
    {
        public ShellView()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            Content.Navigate(typeof(ValidateTabView));
        }
    }
}
