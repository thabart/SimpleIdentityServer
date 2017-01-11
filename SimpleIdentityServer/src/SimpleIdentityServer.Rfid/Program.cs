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

using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using SimpleIdentityServer.Rfid.Common;
using System;

namespace SimpleIdentityServer.Rfid.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Launch signal-r
            using (WebApp.Start<Startup>("http://localhost:8080"))
            {
                Console.WriteLine("Server running at http://localhost:8080/");
                // 2. Launch the listener
                LaunchListener();
                Console.ReadLine();
            }

            Console.ReadLine();
        }

        private static void LaunchListener()
        {
            // INFORMATION ABOUT THE RFID READER.
            // Port : Port_#0004.Hub_#0003
            // VID : FFFF
            // PID : 0035
            var listener = new CardListener();
            listener.Start();
            listener.CardReceived += CardReceived;
        }

        private static void CardReceived(object sender, CardReceivedArgs e)
        {
            Console.WriteLine($"Card number received {e.CardNumber}");
            var rfidHub = GlobalHost.ConnectionManager.GetHubContext<RfidHub>();
            try
            {
                rfidHub.Clients.All.newCard(new RfidCard
                {
                    CardNumber = e.CardNumber,
                    IdentityToken = e.IdentityToken
                });
            }
            catch(Exception)
            {

            }
        }
    }
}
