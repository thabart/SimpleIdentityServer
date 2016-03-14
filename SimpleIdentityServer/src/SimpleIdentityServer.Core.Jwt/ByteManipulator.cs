#region copyright
// Copyright 2015 Habart Thierry
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
using System.Security.Cryptography;

namespace SimpleIdentityServer.Core.Jwt
{
    public static class ByteManipulator
    {
        public static byte[] GenerateRandomBytes(int size)
        {
            var data = new byte[size / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(data);
                return data;
            }
        }

        public static byte[][] SplitByteArrayInHalf(byte[] arr)
        {
            var halfIndex = arr.Length / 2;
            var firstHalf = new byte[halfIndex];
            var secondHalf = new byte[halfIndex];
            Buffer.BlockCopy(arr, 0, firstHalf, 0, halfIndex);
            Buffer.BlockCopy(arr, halfIndex, secondHalf, 0, halfIndex);
            return new[]
            {
                firstHalf,
                secondHalf
            };
        }

        public static byte[] Concat(params byte[][] arrays)
        {
            byte[] result = new byte[arrays.Sum(a => (a == null) ? 0 : a.Length)];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                if (array == null) continue;

                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        public static byte[] LongToBytes(long value)
        {
            ulong _value = (ulong)value;

            return BitConverter.IsLittleEndian
                ? new[] { (byte)((_value >> 56) & 0xFF), (byte)((_value >> 48) & 0xFF), (byte)((_value >> 40) & 0xFF), (byte)((_value >> 32) & 0xFF), (byte)((_value >> 24) & 0xFF), (byte)((_value >> 16) & 0xFF), (byte)((_value >> 8) & 0xFF), (byte)(_value & 0xFF) }
                : new[] { (byte)(_value & 0xFF), (byte)((_value >> 8) & 0xFF), (byte)((_value >> 16) & 0xFF), (byte)((_value >> 24) & 0xFF), (byte)((_value >> 32) & 0xFF), (byte)((_value >> 40) & 0xFF), (byte)((_value >> 48) & 0xFF), (byte)((_value >> 56) & 0xFF) };
        }

        public static bool ConstantTimeEquals(byte[] expected, byte[] actual)
        {
            if (expected == actual)
                return true;

            if (expected == null || actual == null)
                return false;

            if (expected.Length != actual.Length)
                return false;

            bool equals = true;

            for (int i = 0; i < expected.Length; i++)
                if (expected[i] != actual[i])
                    equals = false;

            return equals;
        }

        public static byte[] RightmostBits(byte[] data, int lengthBits)
        {
            var byteCount = lengthBits / 8;
            var result = new byte[byteCount];
            Buffer.BlockCopy(data, data.Length - byteCount, result, 0, byteCount);
            return result;
        }

        public static byte[][] Slice(byte[] array, int count)
        {
            var sliceCount = array.Length / count;
            var result = new byte[sliceCount][];
            for (int i = 0; i < sliceCount; i++)
            {
                var slice = new byte[count];
                Buffer.BlockCopy(array, i * count, slice, 0, count);
                result[i] = slice;
            }

            return result;
        }
    }
}
