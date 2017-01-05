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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleIdentityServer.Rfid
{
    class Program
    {
        const int blockSize = 16;
        const int maxBlocks = 64;
        const int sizeIndex = 4;
        const int startDataIndex = 5;

        static void Main(string[] args)
        {
            // Write and read identity token.
            // WriteAndRead();
            // 1. Launch signal-r
            using (WebApp.Start<Startup>("http://localhost:8080"))
            {
                Console.WriteLine("Server running at http://localhost:8080/");
                // 2. Launch the listener
                LaunchListener();
                Console.ReadLine();
            }
        }

        private static void WriteAndRead()
        {
            const string idToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ";
            WriteToCard(Encoding.UTF8.GetBytes(idToken));
            var result = ReadFromCard();
            Console.WriteLine(Encoding.UTF8.GetString(result.ToArray()));
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
                    CardNumber = e.CardNumber
                });
            }
            catch(Exception)
            {

            }
        }

        private static void WriteToCard(byte[] bytes)
        {
            // ISO Standard 14443A Memory type EEPROM
            // Number of blocks : 64
            // Block size : 16 bytes.
            // More information see : http://blog.pepperl-fuchs.us/high-capacity-rfid-tags
            byte mode = 0x00;
            byte[] buffer = new byte[blockSize * (maxBlocks - startDataIndex)];
            byte[] bufferSize = new byte[blockSize];
            // 1. Check size.
            if (bytes.Length > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("Length is too high");
            }

            var sizeBytes = Encoding.UTF8.GetBytes(bytes.Length.ToString());
            if (sizeBytes.Length > blockSize)
            {
                throw new ArgumentOutOfRangeException("the length size is too high");
            }

            Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
            Buffer.BlockCopy(sizeBytes, 0, bufferSize, 0, sizeBytes.Length);

            // 2. Write the size into the block five.
            Reader.MF_Write(mode, Convert.ToByte(sizeIndex), 1, new byte[]
            {
                0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0,0,0,0,0,0,0,0,0,0
            }, bufferSize);
            Console.WriteLine($"Index {sizeIndex} & block 1 & {string.Join(",", bufferSize)}");

            // 3. Write the data.
            var index = 0;
            for (var blockIndex = startDataIndex + 1; blockIndex <= maxBlocks; blockIndex++)
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
                    Reader.MF_Write(mode, newIndex, 1, new byte[]
                    {
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0,0,0,0,0,0,0,0,0,0
                    }, temp);
                    index++;
                }
                catch
                {
                    return;
                }
            }
        }

        private static IEnumerable<byte> ReadFromCard()
        {
            byte mode = 0x00;
            var bufferSize = new byte[16];
            // 1. Read the size.
            Reader.MF_Read(mode, sizeIndex, 1, new byte[]
            {
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0,0,0,0,0,0,0,0,0,0
            }, bufferSize);
            var length = bufferSize.TakeWhile(b => b != 0).Count();
            var text = Encoding.UTF8.GetString(bufferSize, 0, length);
            int size;
            if (!int.TryParse(text, out size))
            {
                throw new InvalidOperationException("the size cannot be read");
            }

            double numberOfBlocks = Math.Ceiling((double)size / blockSize);
            if (numberOfBlocks > maxBlocks)
            {
                throw new InvalidOperationException("too many blocks");
            }

            // 2. Read the content.
            var result = new List<byte>();
            var index = 0;
            for (var blockIndex = startDataIndex + 1; blockIndex <= numberOfBlocks; blockIndex++)
            {
                try
                {
                    if (blockIndex % 4 == 0)
                    {
                        continue;
                    }

                    var buffer = new byte[16];
                    var newIndex = Convert.ToByte(blockIndex - 1);
                    Reader.MF_Read(mode, newIndex, 1, new byte[]
                    {
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0,0,0,0,0,0,0,0,0,0
                    }, buffer);
                    var nextBlockIndex = blockIndex + 1;
                    if (blockIndex == numberOfBlocks || (nextBlockIndex == numberOfBlocks && nextBlockIndex % 4 == 0))
                    {
                        buffer = buffer.TakeWhile(b => b != 0).ToArray();
                    }

                    result.AddRange(buffer);
                    index++;
                }
                catch
                {
                    return null;
                }
            }

            return result;
        }
    }
}
