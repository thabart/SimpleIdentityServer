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
using System.Linq;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        GrantedToken Execute(ResourceOwnerGrantTypeParameter parameter);
    }

    public class GetTokenByResourceOwnerCredentialsGrantTypeAction : IGetTokenByResourceOwnerCredentialsGrantTypeAction
    {
        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;
        
        private readonly IClientValidator _clientValidator;

        private readonly IScopeValidator _scopeValidator;

        private readonly IResourceOwnerValidator _resourceOwnerValidator;

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public GetTokenByResourceOwnerCredentialsGrantTypeAction(
            IGrantedTokenRepository grantedTokenRepository,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IClientValidator clientValidator,
            IScopeValidator scopeValidator,
            IResourceOwnerValidator resourceOwnerValidator,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource)
        {
            _grantedTokenRepository = grantedTokenRepository;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _clientValidator = clientValidator;
            _scopeValidator = scopeValidator;
            _resourceOwnerValidator = resourceOwnerValidator;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
        }

        public GrantedToken Execute(
            ResourceOwnerGrantTypeParameter parameter)
        {
            var client = _clientValidator.ValidateClientExist(parameter.ClientId);
            if (client == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.ClientIsNotValid, Constants.StandardTokenRequestParameterNames.ClientIdName));
            }

            _resourceOwnerValidator.ValidateResourceOwnerCredentials(parameter.UserName, parameter.Password);

            var allowedTokenScopes = string.Empty;
            if (!string.IsNullOrWhiteSpace(parameter.Scope))
            {
                string messageErrorDescription;
                allowedTokenScopes = string.Join(" ", _scopeValidator.IsScopesValid(parameter.Scope, client, out messageErrorDescription));
                if (!allowedTokenScopes.Any())
                {
                    throw new IdentityServerException(
                        ErrorCodes.InvalidScope,
                        messageErrorDescription);
                }
            }

            // TODO : authenticate the user & create the JWT token
            var generatedToken = _grantedTokenGeneratorHelper.GenerateToken(
                parameter.ClientId,
                allowedTokenScopes);
            _grantedTokenRepository.Insert(generatedToken);

            return generatedToken;
        }
    }
}