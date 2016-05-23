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
using System.Net.Http.Headers;
using SimpleIdentityServer.Core.Api.Token.Actions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.Api.Token
{
    public interface ITokenActions
    {
        GrantedToken GetTokenByResourceOwnerCredentialsGrantType(
            ResourceOwnerGrantTypeParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue);

        GrantedToken GetTokenByAuthorizationCodeGrantType(
            AuthorizationCodeGrantTypeParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue);

        GrantedToken GetTokenByRefreshTokenGrantType(
            RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue);

        GrantedToken GetTokenByClientCredentialsGrantType(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue);

        bool RevokeToken(
            RevokeTokenParameter revokeTokenParameter,
            AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class TokenActions : ITokenActions
    {
        private readonly IGetTokenByResourceOwnerCredentialsGrantTypeAction _getTokenByResourceOwnerCredentialsGrantType;

        private readonly IResourceOwnerGrantTypeParameterValidator _resourceOwnerGrantTypeParameterValidator;

        private readonly IGetTokenByAuthorizationCodeGrantTypeAction _getTokenByAuthorizationCodeGrantTypeAction;

        private readonly IAuthorizationCodeGrantTypeParameterTokenEdpValidator _authorizationCodeGrantTypeParameterTokenEdpValidator;

        private readonly IRefreshTokenGrantTypeParameterValidator _refreshTokenGrantTypeParameterValidator;

        private readonly IGetTokenByRefreshTokenGrantTypeAction _getTokenByRefreshTokenGrantTypeAction;

        private readonly IGetTokenByClientCredentialsGrantTypeAction _getTokenByClientCredentialsGrantTypeAction;

        private readonly IClientCredentialsGrantTypeParameterValidator _clientCredentialsGrantTypeParameterValidator;

        private readonly IRevokeTokenAction _revokeTokenAction;

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        public TokenActions(
            IGetTokenByResourceOwnerCredentialsGrantTypeAction getTokenByResourceOwnerCredentialsGrantType,
            IGetTokenByAuthorizationCodeGrantTypeAction getTokenByAuthorizationCodeGrantTypeAction,
            IResourceOwnerGrantTypeParameterValidator resourceOwnerGrantTypeParameterValidator,
            IAuthorizationCodeGrantTypeParameterTokenEdpValidator authorizationCodeGrantTypeParameterTokenEdpValidator,
            IRefreshTokenGrantTypeParameterValidator refreshTokenGrantTypeParameterValidator,
            IGetTokenByRefreshTokenGrantTypeAction getTokenByRefreshTokenGrantTypeAction,
            IGetTokenByClientCredentialsGrantTypeAction getTokenByClientCredentialsGrantTypeAction,
            IClientCredentialsGrantTypeParameterValidator clientCredentialsGrantTypeParameterValidator,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IRevokeTokenAction revokeTokenAction)
        {
            _getTokenByResourceOwnerCredentialsGrantType = getTokenByResourceOwnerCredentialsGrantType;
            _getTokenByAuthorizationCodeGrantTypeAction = getTokenByAuthorizationCodeGrantTypeAction;
            _resourceOwnerGrantTypeParameterValidator = resourceOwnerGrantTypeParameterValidator;
            _authorizationCodeGrantTypeParameterTokenEdpValidator = authorizationCodeGrantTypeParameterTokenEdpValidator;
            _refreshTokenGrantTypeParameterValidator = refreshTokenGrantTypeParameterValidator;
            _getTokenByRefreshTokenGrantTypeAction = getTokenByRefreshTokenGrantTypeAction;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _getTokenByClientCredentialsGrantTypeAction = getTokenByClientCredentialsGrantTypeAction;
            _clientCredentialsGrantTypeParameterValidator = clientCredentialsGrantTypeParameterValidator;
            _revokeTokenAction = revokeTokenAction;
        }

        public GrantedToken GetTokenByResourceOwnerCredentialsGrantType(
            ResourceOwnerGrantTypeParameter resourceOwnerGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (resourceOwnerGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(resourceOwnerGrantTypeParameter));
            }

            _simpleIdentityServerEventSource.StartGetTokenByResourceOwnerCredentials(resourceOwnerGrantTypeParameter.ClientId,
                resourceOwnerGrantTypeParameter.UserName,
                resourceOwnerGrantTypeParameter.Password);
            _resourceOwnerGrantTypeParameterValidator.Validate(resourceOwnerGrantTypeParameter);
            var result = _getTokenByResourceOwnerCredentialsGrantType.Execute(resourceOwnerGrantTypeParameter,
                authenticationHeaderValue);
            var accessToken = result != null ? result.AccessToken : string.Empty;
            var identityToken = result != null ? result.IdToken : string.Empty;
            _simpleIdentityServerEventSource.EndGetTokenByResourceOwnerCredentials(accessToken, identityToken);
            return result;
        }

        public GrantedToken GetTokenByAuthorizationCodeGrantType(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (authorizationCodeGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationCodeGrantTypeParameter));
            }

            _simpleIdentityServerEventSource.StartGetTokenByAuthorizationCode(
                authorizationCodeGrantTypeParameter.ClientId,
                authorizationCodeGrantTypeParameter.Code);
            _authorizationCodeGrantTypeParameterTokenEdpValidator.Validate(authorizationCodeGrantTypeParameter);
            var result = _getTokenByAuthorizationCodeGrantTypeAction.Execute(authorizationCodeGrantTypeParameter, authenticationHeaderValue);
            _simpleIdentityServerEventSource.EndGetTokenByAuthorizationCode(
                result.AccessToken,
                result.IdToken);
            return result;
        }

        public GrantedToken GetTokenByRefreshTokenGrantType(
            RefreshTokenGrantTypeParameter refreshTokenGrantTypeParameter, 
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (refreshTokenGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(refreshTokenGrantTypeParameter));
            }

            _simpleIdentityServerEventSource.StartGetTokenByRefreshToken(
                refreshTokenGrantTypeParameter.ClientId,
                refreshTokenGrantTypeParameter.RefreshToken);
            _refreshTokenGrantTypeParameterValidator.Validate(refreshTokenGrantTypeParameter);
            var result = _getTokenByRefreshTokenGrantTypeAction.Execute(refreshTokenGrantTypeParameter);
            _simpleIdentityServerEventSource.EndGetTokenByRefreshToken(
                result.AccessToken,
                result.IdToken);
            return result;
        }

        public GrantedToken GetTokenByClientCredentialsGrantType(
            ClientCredentialsGrantTypeParameter clientCredentialsGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (clientCredentialsGrantTypeParameter == null)
            {
                throw new ArgumentNullException(nameof(clientCredentialsGrantTypeParameter));
            }

            _simpleIdentityServerEventSource.StartGetTokenByClientCredentials(clientCredentialsGrantTypeParameter.Scope);
            _clientCredentialsGrantTypeParameterValidator.Validate(clientCredentialsGrantTypeParameter);
            var result = _getTokenByClientCredentialsGrantTypeAction.Execute(clientCredentialsGrantTypeParameter, authenticationHeaderValue);
            _simpleIdentityServerEventSource.EndGetTokenByClientCredentials(
                result.ClientId,
                clientCredentialsGrantTypeParameter.Scope);
            return result;
        }

        public bool RevokeToken(
            RevokeTokenParameter revokeTokenParameter, 
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            if (revokeTokenParameter == null)
            {
                throw new ArgumentNullException(nameof(revokeTokenParameter));
            }

            _simpleIdentityServerEventSource.StartRevokeToken(revokeTokenParameter.Token);
            var result = _revokeTokenAction.Execute(revokeTokenParameter, authenticationHeaderValue);
            _simpleIdentityServerEventSource.EndRevokeToken(revokeTokenParameter.Token);
            return result;
        }
    }
}
