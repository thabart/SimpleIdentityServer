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
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Factories;
using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Core.Results;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Jws.Actions
{
    public interface IGetJwsInformationAction
    {
        Task<JwsInformationResult> Execute(GetJwsParameter getJwsParameter);
    }

    public class GetJwsInformationAction : IGetJwsInformationAction
    {
        private readonly IJwsParser _jwsParser;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IJsonWebKeyConverter _jsonWebKeyConverter;

        #region Constructor

        public GetJwsInformationAction(
            IJwsParser jwsParser,
            IHttpClientFactory httpClientFactory,
            IJsonWebKeyConverter jsonWebKeyConverter)
        {
            _jwsParser = jwsParser;
            _httpClientFactory = httpClientFactory;
            _jsonWebKeyConverter = jsonWebKeyConverter;
        }

        #endregion

        #region Public methods

        public async Task<JwsInformationResult> Execute(GetJwsParameter getJwsParameter)
        {
            if (getJwsParameter == null || string.IsNullOrWhiteSpace(getJwsParameter.Jws))
            {
                throw new ArgumentNullException(nameof(getJwsParameter));
            }

            Uri uri = null;
            if (!string.IsNullOrWhiteSpace(getJwsParameter.Url))
            {
                if (!Uri.TryCreate(getJwsParameter.Url, UriKind.Absolute, out uri))
                {
                    throw new IdentityServerManagerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, getJwsParameter.Url));
                }
            }

            var jws = getJwsParameter.Jws;
            var jwsHeader = _jwsParser.GetHeader(jws);
            if (jwsHeader == null)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheTokenIsNotAValidJws);
            }

            if (uri != null)
            {
                var jsonWebKey = await GetJsonWebKey(jwsHeader.Kid, uri).ConfigureAwait(false);
                if (jsonWebKey == null)
                {
                    throw new IdentityServerManagerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, jwsHeader.Kid, uri.AbsoluteUri));
                }

                var jwsPayload = _jwsParser.ValidateSignature(jws, jsonWebKey);
            }

            return null;
        }

        #endregion

        #region Private methods

        private async Task<JsonWebKey> GetJsonWebKey(string kid, Uri uri)
        {
            try
            {
                var httpClient = _httpClientFactory.GetHttpClient();
                httpClient.BaseAddress = uri;
                var request = await httpClient.GetAsync(uri.AbsoluteUri).ConfigureAwait(false);
                request.EnsureSuccessStatusCode();
                var json = request.Content.ReadAsStringAsync().Result;
                var jsonWebKeySet = json.DeserializeWithJavascript<JsonWebKeySet>();
                var jsonWebKeys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                return jsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            }
            catch (Exception)
            {
                throw new IdentityServerManagerException(
                    ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, uri.AbsoluteUri));
            }
        }

        #endregion
    }
}
