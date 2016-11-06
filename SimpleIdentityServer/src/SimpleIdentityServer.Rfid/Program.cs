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

using SimpleIdentityServer.Rfid.Menu;
using System;
using System.Text;

namespace SimpleIdentityServer.Rfid
{
    class Program
    {
        static void Main(string[] args)
        {
            // Port : Port_#0004.Hub_#0003
            // VID : FFFF
            // PID : 0035
            var b = GetIdToken();
            Console.Title = "RFID reader & writer";
            LaunchListener();
            Console.ReadLine();
        }

        private static void LaunchListener()
        {
            var listener = new CardListener();
            listener.Start();
            listener.CardReceived += CardReceived;
            Console.WriteLine("Press a key to exit the application ...");
        }

        private static void CardReceived(object sender, CardReceivedArgs e)
        {
            Console.WriteLine($"Card number received {e.CardNumber}");
        }

        private static void LaunchMenu()
        {
            var home = new ChoiceMenuItem();
            var reader = new ChoiceMenuItem("Execute system commands");
            reader.Add(new ReaderSerialNumberMenuItem());
            home.Add(reader);
            home.Execute();
        }

        private static void WriteIdentityToken(byte[] bytes)
        {
            byte mode = 0x00;
            // int nRet = Reader.MF_Write(mode, blk_add, num_blk, snr, buffer);
        }

        static byte[] GetIdToken()
        {
            const string idToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ";
            return  Encoding.UTF8.GetBytes(idToken);
        }
    }
}
