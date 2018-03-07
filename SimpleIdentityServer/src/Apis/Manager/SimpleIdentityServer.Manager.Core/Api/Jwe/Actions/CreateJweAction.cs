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

using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Helpers;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Jwe.Actions
{
    public interface ICreateJweAction
    {
        Task<string> ExecuteAsync(CreateJweParameter createJweParameter);
    }

    public class CreateJweAction : ICreateJweAction
    {
        private readonly IJweGenerator _jweGenerator;
        private IJsonWebKeyHelper _jsonWebKeyHelper;

        public CreateJweAction(
            IJweGenerator jweGenerator,
            IJsonWebKeyHelper jsonWebKeyHelper)
        {
            _jweGenerator = jweGenerator;
            _jsonWebKeyHelper = jsonWebKeyHelper;
        }
        
        public async Task<string> ExecuteAsync(CreateJweParameter createJweParameter)
        {
            if (createJweParameter == null)
            {
                throw new ArgumentNullException(nameof(createJweParameter));
            }

            if (string.IsNullOrWhiteSpace(createJweParameter.Url))
            {
                throw new ArgumentNullException(nameof(createJweParameter.Url));
            }

            if (string.IsNullOrWhiteSpace(createJweParameter.Jws))
            {
                throw new ArgumentNullException(nameof(createJweParameter.Jws));
            }

            if (string.IsNullOrWhiteSpace(createJweParameter.Kid)) 
            {
                throw new ArgumentNullException(nameof(createJweParameter.Kid));
            }

            Uri uri = null;
            if (!Uri.TryCreate(createJweParameter.Url, UriKind.Absolute, out uri))
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, createJweParameter.Url));
            }

            var jsonWebKey = await _jsonWebKeyHelper.GetJsonWebKey(createJweParameter.Kid, uri).ConfigureAwait(false);
            if (jsonWebKey == null)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, createJweParameter.Kid, uri.AbsoluteUri));
            }

            var result = string.Empty;
            if (!string.IsNullOrWhiteSpace(createJweParameter.Password))
            {
                result = _jweGenerator.GenerateJweByUsingSymmetricPassword(createJweParameter.Jws,
                    createJweParameter.Alg,
                    createJweParameter.Enc,
                    jsonWebKey,
                    createJweParameter.Password);
            }
            else
            {
                result = _jweGenerator.GenerateJwe(createJweParameter.Jws,
                    createJweParameter.Alg,
                    createJweParameter.Enc,
                    jsonWebKey);
            }

            return result;
        }
    }
}
