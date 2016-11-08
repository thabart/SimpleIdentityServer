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

using System.Text;

namespace SimpleIdentityServer.Rfid.Card
{
    internal static class CardReaderHelper
    {
        public static string GetSerialNumberCard(out int error)
        {
            byte mode = 0x26, halt = 0x00;
            byte[] snr = new byte[1], value = new byte[4];
            error = Reader.MF_Getsnr(mode, halt, snr, value);
            if (error != 0)
            {
                return null;
            }
            else
            {
                return ToStr(value);
            }
        }

        private static string ToStr(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.AppendFormat("{0:X2}", b);
            }

            return builder.ToString();
        }
    }
}
