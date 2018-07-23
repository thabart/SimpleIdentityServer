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

using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Common.Extensions;
using System;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Client.Builders
{
    public class PKCE
    {
        public string CodeVerifier { get; set; }
        public string CodeChallenge { get; set; }
    }

    public class PkceBuilder
    {
        public PKCE Build(CodeChallengeMethods method)
        {
            var result = new PKCE();
            result.CodeVerifier = GetCodeVerifier();
            result.CodeChallenge = GetCodeChallenge(result.CodeVerifier, method);
            return result;
        }

        private static string GetCodeVerifier()
        {
            const string possibleChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~";
            var random = new Random();
            var nb = random.Next(43, 128);
            var result = new StringBuilder();
            for (var i = 0; i < nb; i++)
            {
                result.Append(possibleChars[random.Next(possibleChars.Length)]);
            }

            return result.ToString();
        }

        private static string GetCodeChallenge(string codeVerifier, CodeChallengeMethods method)
        {
            if (method == CodeChallengeMethods.Plain)
            {
                return codeVerifier;
            }

            var hashed = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
            return hashed.Base64EncodeBytes();
        }
    }
}
