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

using SimpleIdentityServer.Core.Common.Extensions;
using System.Linq;
using System.Net.Http.Headers;

namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IAuthenticateInstructionGenerator
    {
        AuthenticateInstruction GetAuthenticateInstruction(AuthenticationHeaderValue authenticationHeaderValue);
    }

    internal class AuthenticateInstructionGenerator : IAuthenticateInstructionGenerator
    {
        #region Public methods

        public AuthenticateInstruction GetAuthenticateInstruction(AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = new AuthenticateInstruction();
            if (authenticationHeaderValue != null
                && !string.IsNullOrWhiteSpace(authenticationHeaderValue.Parameter))
            {
                var parameters = GetParameters(authenticationHeaderValue.Parameter);
                if (parameters != null && parameters.Count() == 2)
                {
                    result.ClientIdFromAuthorizationHeader = parameters[0];
                    result.ClientSecretFromAuthorizationHeader = parameters[1];
                }
            }

            return result;
        }

        #endregion

        #region Private methods
        
        private static string[] GetParameters(string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                return new string[0];
            }

            var decodedParameter = authorizationHeaderValue.Base64Decode();
            return decodedParameter.Split(':');
        }

        #endregion
    }
}
