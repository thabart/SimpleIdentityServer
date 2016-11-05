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

namespace SimpleIdentityServer.Rfid
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "RFID reader & writer";
            var home = new ChoiceMenuItem();
            var reader = new ChoiceMenuItem("Execute system commands");
            reader.Add(new ReaderSerialNumberMenuItem());
            home.Add(reader);
            home.Execute();
            Console.ReadLine();
            /*
            var serialNumber = new byte[9];
            // 1. Get serialize number.
            var number = Reader.GetSerNum(serialNumber);
            // 2. Control the led.
            byte[] buffer = new byte[1];
            var result = Reader.ControlLED(20, 6, buffer);
            Console.ReadLine();
            */
        }
    }
}
