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

using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Helpers;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Jws.Actions
{
    public interface ICreateJwsAction
    {
        Task<string> Execute(CreateJwsParameter createJwsParameter);
    }

    public class CreateJwsAction : ICreateJwsAction
    {
        private readonly IJwsGenerator _jwsGenerator;

        private readonly IJsonWebKeyHelper _jsonWebKeyHelper;

        #region Constructor

        public CreateJwsAction(
            IJwsGenerator jwsGenerator,
            IJsonWebKeyHelper jsonWebKeyHelper)
        {
            _jwsGenerator = jwsGenerator;
            _jsonWebKeyHelper = jsonWebKeyHelper;
        }

        #endregion

        #region Public methods

        public async Task<string> Execute(CreateJwsParameter createJwsParameter)
        {
            if (createJwsParameter == null)
            {
                throw new ArgumentNullException(nameof(createJwsParameter));
            }


            if(createJwsParameter.Payload == null
                || !createJwsParameter.Payload.Any())
            {
                throw new ArgumentNullException(nameof(createJwsParameter.Payload));
            }

            if (createJwsParameter.Alg != JwsAlg.none &&
                (string.IsNullOrWhiteSpace(createJwsParameter.Kid) || string.IsNullOrWhiteSpace(createJwsParameter.Url)))
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheJwsCannotBeGeneratedBecauseMissingParameters);
            }

            Uri uri = null;
            if (createJwsParameter.Alg != JwsAlg.none && !Uri.TryCreate(createJwsParameter.Url, UriKind.Absolute, out uri))
            {
                throw new IdentityServerManagerException(ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheUrlIsNotWellFormed);
            }

            JsonWebKey jsonWebKey = null;
            if (createJwsParameter.Alg != JwsAlg.none)
            {
                jsonWebKey = await _jsonWebKeyHelper.GetJsonWebKey(createJwsParameter.Kid, uri).ConfigureAwait(false);
                if (jsonWebKey == null)
                {
                    throw new IdentityServerManagerException(
                        ErrorCodes.InvalidRequestCode,
                        string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, createJwsParameter.Kid, uri.AbsoluteUri));
                }
            }

            return _jwsGenerator.Generate(createJwsParameter.Payload,
                createJwsParameter.Alg,
                jsonWebKey);
        }

        #endregion
    }
}
