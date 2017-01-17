using Caliburn.Micro;
using Windows.UI.Xaml.Controls;

namespace RfidValidator.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private INavigationService _navigationService;

        public ShellViewModel(WinRTContainer container, IEventAggregator eventAggregator)
        {
            _container = container;
            _eventAggregator = eventAggregator;
        }

        public void SetupNavigationService(Frame frame)
        {
            if (_container.HasHandler(typeof(INavigationService), null))
            {
                _container.UnregisterHandler(typeof(INavigationService), null);
            }

            _navigationService = _container.RegisterNavigationService(frame);
        }

        public void ShowValidate()
        {
            _navigationService.For<ValidateTabViewModel>().Navigate();
        }

        public void ShowSubscription()
        {
            _navigationService.For<AccountTabViewModel>().Navigate();
        }
    }
}
