using System;
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
        private string _cardNumber;
        private readonly CancellationTokenSource _token;
        private readonly Task _task;
        private readonly RfidRc522 _rfidRc522;

        public CardListener()
        {
            _cardNumber = string.Empty;
            _task = ListenCard();
            _token = new CancellationTokenSource();
            _rfidRc522 = new RfidRc522();
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
                await _rfidRc522.Start();
                while (true)
                {
                    if (_token.IsCancellationRequested)
                    {
                        break;
                    }

                    if (_rfidRc522.IsTagPresent())
                    {
                        Uid uid = _rfidRc522.ReadUid();
                        var b = _rfidRc522.SelectTag(uid);
                        var size = _rfidRc522.ReadBlock(4, uid, new byte[] {
                            0xFF,0xFF,0xFF,0xFF,0xFF,0xFF
                        });
                        _rfidRc522.HaltTag();
                    }

                    /*
                    var cardNumber = RfidManager.GetSerialNumberCard();
                    if (string.IsNullOrWhiteSpace(cardNumber))
                    {
                        _cardNumber = string.Empty;
                    }
                    else
                    {
                        if (cardNumber != _cardNumber)
                        {
                            _cardNumber = cardNumber;
                            if (CardReceived != null)
                            {
                                try
                                {
                                    var buffer = RfidManager.ReadFromCard();
                                    CardReceived(this, new CardReceivedArgs(cardNumber, Encoding.UTF8.GetString(buffer.ToArray())));
                                }
                                catch
                                {
                                }
                            }

                            _cardNumber = cardNumber;
                        }
                    }
                    */

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
