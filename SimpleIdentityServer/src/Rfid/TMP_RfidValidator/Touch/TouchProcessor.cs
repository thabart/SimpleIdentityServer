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
        private readonly CoreDispatcher _dispatcher;
        private CancellationTokenSource _cancellationToken;

        public TouchProcessor(ITouchDevice touchDevice, CoreDispatcher dispatcher)
        {
            _touchDevice = touchDevice;
            _dispatcher = dispatcher;
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

        public void ReadTouch()
        {
            var touchPosition = _touchDevice.ReadTouchPoints();
            int pressure = _touchDevice.Pressure;
            if (pressure > 5)
            {
                if (!_penPressed)
                {
                    _penPressed = true;
                    var _ = _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PointerDown?.Invoke(this, new PointerEventArgs() { Position = touchPosition, Pressure = pressure });
                    });
                }
                else
                {
                    var _ = _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        PointerMoved?.Invoke(this, new PointerEventArgs() { Position = touchPosition, Pressure = pressure });
                    });
                }
            }
            else if (pressure < 2 && _penPressed == true)
            {
                _penPressed = false;
                var _ = _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    PointerUp?.Invoke(this, new PointerEventArgs() { Position = touchPosition });
                });
            }
        }
    }
}
