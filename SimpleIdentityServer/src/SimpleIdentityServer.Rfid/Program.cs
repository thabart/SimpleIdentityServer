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
using SimpleIdentityServer.Rfid.Card;
using SimpleIdentityServer.Rfid.Hubs;
using SimpleIdentityServer.Rfid.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleIdentityServer.Rfid
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Write identity token
            // WriteIdentityToken(GetIdToken());
            // 2. Read identity token
            var expected = GetIdToken();
            var returned = GetIdentityToken().ToArray();
            for (int i = 0; i < expected.Length; i++)
            {
                Console.WriteLine($"expected : {expected[i]} returned : {returned[i]}");
            }

            Console.ReadLine();
            /*
            // 1. Launch signal-r
            using (WebApp.Start<Startup>("http://localhost:8080"))
            {
                Console.WriteLine("Server running at http://localhost:8080/");
                // 2. Launch the listener
                LaunchListener();
                Console.ReadLine();
            }
            */
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
                    CardNumber = e.CardNumber
                });
            }
            catch(Exception)
            {

            }
        }

        private static void LaunchMenu()
        {
            var home = new ChoiceMenuItem();
            var reader = new ChoiceMenuItem("Execute system commands");
            reader.Add(new ReaderSerialNumberMenuItem());
            home.Add(reader);
            home.Execute();
        }

        private static IEnumerable<byte> GetIdentityToken()
        {
            var lst = new List<byte>();
            byte mode = 0x00;
            int numberOfBlocks = 64, skippedBlocks = 4; // Ignore the first sector reserved to the manufacturer.
            var index = 0;
            for (var blockIndex = skippedBlocks + 1; blockIndex <= numberOfBlocks; blockIndex++)
            {
                try
                {
                    if (blockIndex % 4 == 0)
                    {
                        continue;
                    }

                    var buffer = new byte[16];
                    var newIndex = Convert.ToByte(blockIndex - 1);
                    int nRet = Reader.MF_Read(mode, newIndex, 1, new byte[]
                    {
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0,0,0,0,0,0,0,0,0,0
                    }, buffer);
                    lst.AddRange(buffer);
                    index++;
                }
                catch (Exception)
                {
                    string s = "";
                }
            }

            return lst;
        }

        private static void WriteIdentityToken(byte[] bytes)
        {
            // ISO Standard 14443A Memory type EEPROM
            // Number of blocks : 64
            // Block size : 16 bytes.
            // More information see : http://blog.pepperl-fuchs.us/high-capacity-rfid-tags
            byte mode = 0x00;
            int numberOfBlocks = 64, skippedBlocks = 4; // Ignore the first sector reserved to the manufacturer.
            byte[] buffer = new byte[16 * (numberOfBlocks - skippedBlocks)];
            if (bytes.Length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("Length is too high");
            }

            Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
            var index = 0;
            // TODO : Write the data size.
            /*
            int nRet = Reader.MF_Write(mode, Convert.ToByte(skippedBlocks), 1, new byte[]
            {
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0,0,0,0,0,0,0,0,0,0
            }, temp);
            */

            for (var blockIndex = skippedBlocks + 1; blockIndex <= numberOfBlocks; blockIndex++)
            {
                try
                {
                    if (blockIndex % 4 == 0)
                    {
                        continue;
                    }

                    var temp = buffer.Skip(index * 16).Take(16).ToArray();
                    var newIndex = Convert.ToByte(blockIndex - 1);
                    Console.WriteLine($"Index {newIndex} & block 1 & {string.Join(",", temp)}");
                    nRet = Reader.MF_Write(mode, newIndex, 1, new byte[]
                    {
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0,0,0,0,0,0,0,0,0,0
                    }, temp);
                    index++;
                }
                catch (Exception)
                {
                    string s = "";
                }
            }
        }

        static byte[] GetIdToken()
        {
            const string idToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ";
            return  Encoding.UTF8.GetBytes(idToken);
        }
    }
}
