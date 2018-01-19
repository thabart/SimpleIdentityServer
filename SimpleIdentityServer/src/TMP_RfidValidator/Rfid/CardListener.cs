using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RfidValidator.Rfid
{
    public class CardReceivedArgs : EventArgs
    {
        public CardReceivedArgs(string cardNumber, string identityToken)
        {
            CardNumber = cardNumber;
            IdentityToken = identityToken;
        }

        public string CardNumber { get; private set; }
        public string IdentityToken { get; private set; }
    }

    public class CardListener : IDisposable
    {
        private RfidManager _rfidManager;
        private string _cardNumber;
        private readonly CancellationTokenSource _token;
        private readonly Task _task;

        public CardListener()
        {
            _cardNumber = string.Empty;
            _task = ListenCard();
            _token = new CancellationTokenSource();
            _rfidManager = new RfidManager();
        }

        public event EventHandler<CardReceivedArgs> CardReceived;

        public void Start()
        {
            _task.Start();
        }

        public void End()
        {
            _token.Cancel();
        }

        private Task ListenCard()
        {
            return new Task(async () =>
            {
#if ARM
                await _rfidManager.Start();
#endif
                while (true)
                {
                    if (_token.IsCancellationRequested)
                    {
                        break;
                    }

                    var cardNumber = _rfidManager.GetSerialNumberCard();
                    if (string.IsNullOrEmpty(cardNumber))
                    {
                        _cardNumber = string.Empty;
                    }
                    else
                    {
                        if (cardNumber != _cardNumber)
                        {
                            if (CardReceived != null)
                            {
                                try
                                {
                                    var buffer = _rfidManager.ReadFromCard();
                                    CardReceived(this, new CardReceivedArgs(cardNumber, Encoding.UTF8.GetString(buffer.ToArray())));
                                    _cardNumber = cardNumber;
                                }
                                catch(Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
#if ARM
                                    _rfidManager.Stop();
#endif
                                }
                            }
                        }
                    }

                    await Task.Delay(1000);
                }
            });
        }

        public void Dispose()
        {
            _token.Cancel();
        }
    }
}
