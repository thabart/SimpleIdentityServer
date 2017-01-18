using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace RfidValidator.Touch
{
    public sealed class PointerEventArgs : EventArgs
    {
        public Point Position { get; internal set; }
        public int Pressure { get; internal set; }
    }

    public class TouchProcessor
    {
        private bool _penPressed;
        private readonly ITouchDevice _touchDevice;
        private CancellationTokenSource _cancellationToken;

        public TouchProcessor(ITouchDevice touchDevice)
        {
            _touchDevice = touchDevice;
            _penPressed = false;
        }

        public event EventHandler<PointerEventArgs> PointerDown;
        public event EventHandler<PointerEventArgs> PointerUp;
        public event EventHandler<PointerEventArgs> PointerMoved;

        public void Start()
        {
            _cancellationToken = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while(!_cancellationToken.IsCancellationRequested)
                {
                    ReadTouch();
                    await Task.Delay(10);
                }
            });
        }

        private void ReadTouch()
        {
            var touchPosition = _touchDevice.ReadTouchPoints();
            int pressure = _touchDevice.Pressure;
            if (pressure > 5)
            {
                if (!_penPressed)
                {
                    _penPressed = true;
                    var dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;
                    var _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        PointerDown?.Invoke(this, new PointerEventArgs() { Position = touchPosition, Pressure = pressure });
                    });
                }
                /*
                else
                {
                    var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        _PointerMoved?.Invoke(this, new PointerEventArgs() { Position = device.TouchPosition, Pressure = pressure });
                    });
                }*/
            }
            /*
            else if (pressure < 2 && penPressed == true)
            {
                penPressed = false;
                var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _PointerUp?.Invoke(this, new PointerEventArgs() { Position = device.TouchPosition });
                });
            }*/
        }
    }
}
