using Caliburn.Micro;
using RfidValidator.Rfid;
#if ARM
using RfidValidator.Touch;
#endif
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace RfidValidator.ViewModels
{
    public class ShellViewModel : Screen
    {
        public const UInt16 UsagePage = 0xFF00;
        public const UInt16 UsageId = 0x01;
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
#if ARM
            InitializeTouch();
#endif
        }

        public void ShowValidate()
        {
            _navigationService.For<ValidateTabViewModel>().Navigate();
        }

        public void ShowSubscription()
        {
            _navigationService.For<AccountTabViewModel>().Navigate();
        }

#if ARM
        private async Task InitializeTouch()
        {
            var touchScreen = await TouchDevice.Get();
            var touchProcessor = new TouchProcessor(touchScreen);
            touchProcessor.Start();
        }
#endif
    }
}
