#region copyright
// Copyright 2017 Habart Thierry
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleIdentityServer.Rfid.Common
{
    public static class RfidManager
    {
        const int blockSize = 16;
        const int maxBlocks = 64;
        const int sizeIndex = 4;
        const int startDataIndex = 5;

        public static void WriteToCard(byte[] bytes)
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
            // Console.WriteLine($"Index {sizeIndex} & block 1 & {string.Join(",", bufferSize)}");

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
                    // Console.WriteLine($"Index {newIndex} & block 1 & {string.Join(",", temp)}");
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

        public static IEnumerable<byte> ReadFromCard()
        {
            byte mode = 0x00;
            var bufferSize = new byte[16];
            // 1. Read the size.
            Reader.MF_Read(mode, Convert.ToByte(sizeIndex), 1, new byte[]
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

            double t = (double)size / (double)blockSize;
            double numberOfBlocks = Math.Ceiling((double)size / blockSize);
            if (numberOfBlocks > maxBlocks)
            {
                throw new InvalidOperationException("too many blocks");
            }

            // 2. Read the content.
            var result = new List<byte>();
            var index = 1;
            for (var blockIndex = startDataIndex + 1; index <= numberOfBlocks; blockIndex++)
            {
                try
                {
                    if (blockIndex % 4 == 0)
                    {
                        continue;
                    }

                    var buffer = new byte[16];
                    var newIndex = Convert.ToByte(blockIndex - 1);
                    // Console.WriteLine($"Index {newIndex}");
                    Reader.MF_Read(mode, newIndex, 1, new byte[]
                    {
                        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0,0,0,0,0,0,0,0,0,0
                    }, buffer);
                    var nextBlockIndex = blockIndex + 1;
                    if (index == numberOfBlocks || (index + 1 == numberOfBlocks && nextBlockIndex % 4 == 0))
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

        public static string GetSerialNumberCard()
        {
            byte mode = 0x26, halt = 0x00;
            byte[] snr = new byte[1], value = new byte[4];
            var error = Reader.MF_Getsnr(mode, halt, snr, value);
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
