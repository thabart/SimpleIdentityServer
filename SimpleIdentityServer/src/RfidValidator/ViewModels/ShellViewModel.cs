using Caliburn.Micro;
using RfidValidator.Touch;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace RfidValidator.ViewModels
{
    public class ShellViewModel : Screen
    {
        public const UInt16 UsagePage = 0xFF00;
        public const UInt16 UsageId = 0x01;
        private Frame _frame;
        private UIElement _shellView;
        private Point _lastPosition = new Point(double.NaN, double.NaN);
        private IScrollProvider _currentScrollItem;
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private INavigationService _navigationService;

        public ShellViewModel(WinRTContainer container, IEventAggregator eventAggregator)
        {
            _container = container;
            _eventAggregator = eventAggregator;
        }

        public void SetupNavigationService(Frame frame, UIElement shellView)
        {
            _frame = frame;
            _shellView = shellView;
            if (_container.HasHandler(typeof(INavigationService), null))
            {
                _container.UnregisterHandler(typeof(INavigationService), null);
            }

            _navigationService = _container.RegisterNavigationService(frame);
#if ARM
            var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            InitializeTouch(dispatcher);
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
        
        private async Task InitializeTouch(CoreDispatcher dispatcher)
        {
            var touchScreen = await TouchDevice.Get();
            var touchProcessor = new TouchProcessor(touchScreen, dispatcher);
            touchProcessor.PointerDown += OnPointerDown;
            touchProcessor.PointerMoved += OnPointerMoved;
            touchProcessor.PointerUp += OnPointerUp;
            touchProcessor.Start();
        }

        private void OnPointerDown(object sender, Touch.PointerEventArgs e)
        {
            _currentScrollItem = FindElementsToInvoke(e.Position);
            _lastPosition = e.Position;
        }

        private void OnPointerMoved(object sender, Touch.PointerEventArgs e)
        {
            if (_currentScrollItem != null)
            {
                double dx = e.Position.X - _lastPosition.X;
                double dy = e.Position.Y - _lastPosition.Y;
                if (!_currentScrollItem.HorizontallyScrollable) dx = 0;
                if (!_currentScrollItem.VerticallyScrollable) dy = 0;

                Windows.UI.Xaml.Automation.ScrollAmount h = Windows.UI.Xaml.Automation.ScrollAmount.NoAmount;
                Windows.UI.Xaml.Automation.ScrollAmount v = Windows.UI.Xaml.Automation.ScrollAmount.NoAmount;
                if (dx < 0) h = Windows.UI.Xaml.Automation.ScrollAmount.SmallIncrement;
                else if (dx > 0) h = Windows.UI.Xaml.Automation.ScrollAmount.SmallDecrement;
                if (dy < 0) v = Windows.UI.Xaml.Automation.ScrollAmount.SmallIncrement;
                else if (dy > 0) v = Windows.UI.Xaml.Automation.ScrollAmount.SmallDecrement;
                _currentScrollItem.Scroll(h, v);
            }

            _lastPosition = e.Position;
        }

        private void OnPointerUp(object sender, Touch.PointerEventArgs e)
        {
            _currentScrollItem = null;
        }

        private IScrollProvider FindElementsToInvoke(Point screenPosition)
        {
            var elements = VisualTreeHelper.FindElementsInHostCoordinates(new Point(screenPosition.X, screenPosition.Y), _shellView, false);
            foreach (var e in elements.OfType<FrameworkElement>())
            {
                var element = e;
                AutomationPeer peer = null;
                object pattern = null;
                while (true)
                {
                    peer = FrameworkElementAutomationPeer.FromElement(element);
                    if (peer != null)
                    {
                        pattern = peer.GetPattern(PatternInterface.Invoke);
                        if (pattern != null)
                        {
                            break;
                        }

                        pattern = peer.GetPattern(PatternInterface.Scroll);
                        if (pattern != null)
                        {
                            break;
                        }
                    }
                    var parent = VisualTreeHelper.GetParent(element);
                    if (parent is FrameworkElement)
                    {
                        element = parent as FrameworkElement;
                    }
                    else
                    {
                        break;
                    }
                }

                if (pattern != null)
                {
                    var p = pattern as IInvokeProvider;
                    p?.Invoke();
                    return pattern as IScrollProvider;
                }
            }

            return null;
        }
    }
}
