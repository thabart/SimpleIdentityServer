using System.Threading;
using System.Threading.Tasks;

namespace RfidValidator.Touch
{
    public class TouchProcessor
    {
        private readonly ITouchDevice _touchDevice;
        private CancellationTokenSource _cancellationToken;

        public TouchProcessor(ITouchDevice touchDevice)
        {
            _touchDevice = touchDevice;
        }

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
            _touchDevice.ReadTouchPoints();

        }
    }
}
