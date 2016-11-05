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
using System.Text;

namespace SimpleIdentityServer.Rfid.Menu
{
    internal class ReaderSerialNumberMenuItem : BaseMenuItem
    {
        public ReaderSerialNumberMenuItem()
        {
            Title = "Read serial number";
        }

        public override void Execute()
        {
            var serialNumber = new byte[9];
            var number = Reader.GetSerNum(serialNumber);
            var builder = new StringBuilder();
            foreach(var b in serialNumber)
            {
                builder.AppendFormat("{0:X2}", b);
            }

            Console.WriteLine($"The serial number is : \t {builder.ToString()}");
        }
    }
}
