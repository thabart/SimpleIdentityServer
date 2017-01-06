#region copyright
// Copyright 2016 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Rfid.Card
{
    internal class CardReceivedArgs : EventArgs
    {
        public CardReceivedArgs(string cardNumber, string identityToken)
        {
            CardNumber = cardNumber;
            IdentityToken = identityToken;
        }

        public string CardNumber { get; private set; }
        public string IdentityToken { get; private set; }
    }

    internal class CardListener : IDisposable
    {
        private string _cardNumber;
        private readonly CancellationTokenSource _token;
        private readonly Task _task;

        public CardListener()
        {
            _cardNumber = string.Empty;
            _task = ListenCard();
            _token = new CancellationTokenSource();
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
            return new Task(() =>
            {
                while(true)
                {
                    if (_token.IsCancellationRequested)
                    {
                        Console.WriteLine("Task cancelled");
                        break;
                    }

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
                                    Console.WriteLine("The identity token cannot be read");
                                }
                            }

                            _cardNumber = cardNumber;
                        }
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        public void Dispose()
        {
            _token.Cancel();
        }
    }
}
