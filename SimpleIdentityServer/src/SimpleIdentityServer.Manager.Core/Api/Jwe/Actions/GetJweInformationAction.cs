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
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Helpers;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Jwe.Actions
{
    public interface IGetJweInformationAction
    {
        Task<JweInformationResult> ExecuteAsync(GetJweParameter getJweParameter);
    }

    public class GetJweInformationAction : IGetJweInformationAction
    {
        private readonly IJweParser _jweParser;
        private readonly IJwsParser _jwsParser;
        private readonly IJsonWebKeyHelper _jsonWebKeyHelper;

        public GetJweInformationAction(
            IJweParser jweParser,
            IJwsParser jwsParser,
            IJsonWebKeyHelper jsonWebKeyHelper)
        {
            _jweParser = jweParser;
            _jwsParser = jwsParser;
            _jsonWebKeyHelper = jsonWebKeyHelper;
        }

        public async Task<JweInformationResult> ExecuteAsync(GetJweParameter getJweParameter)
        {
            if (getJweParameter == null)
            {
                throw new ArgumentNullException(nameof(getJweParameter));
            }

            if (string.IsNullOrWhiteSpace(getJweParameter.Jwe))
            {
                throw new ArgumentNullException(nameof(getJweParameter.Jwe));
            }

            if (string.IsNullOrWhiteSpace(getJweParameter.Url))
            {
                throw new ArgumentNullException(nameof(getJweParameter.Url));
            }
            
            Uri uri = null;
            if (!Uri.TryCreate(getJweParameter.Url, UriKind.Absolute, out uri))
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, getJweParameter.Url));
            }

            var jwe = getJweParameter.Jwe;
            var jweHeader = _jweParser.GetHeader(jwe);
            if (jweHeader == null)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheTokenIsNotAValidJwe);
            }

            var jsonWebKey = await _jsonWebKeyHelper.GetJsonWebKey(jweHeader.Kid, uri).ConfigureAwait(false);
            if (jsonWebKey == null)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, jweHeader.Kid, uri.AbsoluteUri));
            }

            var content = string.Empty;
            if (!string.IsNullOrWhiteSpace(getJweParameter.Password))
            {
                content = _jweParser.ParseByUsingSymmetricPassword(jwe, jsonWebKey, getJweParameter.Password);
            }
            else
            {
                content = _jweParser.Parse(jwe, jsonWebKey);                
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheContentCannotBeExtractedFromJweToken);
            }

            var result = new JweInformationResult
            {
                Content = content,
                IsContentJws = false
            };

            var jwsHeader = _jwsParser.GetHeader(content);
            if (jwsHeader != null)
            {
                result.IsContentJws = true;
            }
                        
            return result;
        }
    }
}
