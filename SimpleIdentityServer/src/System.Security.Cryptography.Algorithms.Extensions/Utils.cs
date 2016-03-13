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

namespace System.Security.Cryptography
{
    internal static class Utils
    {
        internal static string DiscardWhiteSpaces(string inputBuffer)
        {
            return DiscardWhiteSpaces(inputBuffer, 0, inputBuffer.Length);
        }

        internal static string DiscardWhiteSpaces(string inputBuffer, int inputOffset, int inputCount)
        {
            int i, iCount = 0;
            for (i = 0; i < inputCount; i++)
            {
                if (char.IsWhiteSpace(inputBuffer[inputOffset + i]))
                {
                    iCount++;
                }
            }

            var output = new char[inputCount - iCount];
            iCount = 0;
            for (i = 0; i < inputCount; i++)
            {
                if (!char.IsWhiteSpace(inputBuffer[inputOffset + i]))
                {
                    output[iCount++] = inputBuffer[inputOffset + i];
                }
            }

            return new string(output);
        }
    }
}
