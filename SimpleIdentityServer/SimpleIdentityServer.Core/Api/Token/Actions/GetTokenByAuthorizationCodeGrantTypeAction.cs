using System;
using System.Net.Http.Headers;

using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Api.Token.Actions
{
    public interface IGetTokenByAuthorizationCodeGrantTypeAction
    {
        GrantedToken Execute(AuthorizationCodeGrantTypeParameter parameter,
            AuthenticationHeaderValue authenticationHeaderValue);
    }

    public class GetTokenByAuthorizationCodeGrantTypeAction : IGetTokenByAuthorizationCodeGrantTypeAction
    {
        private readonly IClientValidator _clientValidator;

        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;

        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        private readonly ITokenHelper _tokenHelper;

        public GetTokenByAuthorizationCodeGrantTypeAction(
            IClientValidator clientValidator,
            IAuthorizationCodeRepository authorizationCodeRepository,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            ITokenHelper tokenHelper)
        {
            _clientValidator = clientValidator;
            _authorizationCodeRepository = authorizationCodeRepository;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _tokenHelper = tokenHelper;
        }

        public GrantedToken Execute(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter, 
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var authorizationCode = ValidateParameter(
                authorizationCodeGrantTypeParameter, 
                authenticationHeaderValue);

            // Remove the authorization code. Don't reuse it !
            _authorizationCodeRepository.RemoveAuthorizationCode(authorizationCode.Code);
            var grantedToken = _tokenHelper.GenerateToken(authorizationCode.Scopes);
            grantedToken.IdToken = authorizationCode.IdToken;
            return grantedToken;
        }

        /// <summary>
        /// Check the parameters based on the RFC : http://openid.net/specs/openid-connect-core-1_0.html#TokenRequestValidation
        /// </summary>
        /// <param name="authorizationCodeGrantTypeParameter"></param>
        /// <param name="authenticationHeaderValue"></param>
        /// <returns></returns>
        private AuthorizationCode ValidateParameter(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var client = _clientValidator.ValidateClientExist(authorizationCodeGrantTypeParameter.ClientId);
            if (client == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.ClientIsNotValid, "client_id"));
            }

            // Authenticate the client
            var isAuthenticated = false;
            if (authenticationHeaderValue != null)
            {
                isAuthenticated = _clientValidator.ValidateClientIsAuthenticated(
                authorizationCodeGrantTypeParameter.ClientId,
                authorizationCodeGrantTypeParameter.ClientSecret,
                authenticationHeaderValue.Parameter);
            }

            if (!isAuthenticated)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    ErrorDescriptions.TheClientCannotBeAuthenticated);
            }

            var authorizationCode = _authorizationCodeRepository.GetAuthorizationCode(authorizationCodeGrantTypeParameter.Code);
            // Check if the authorization code is valid
            if (authorizationCode == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsNotCorrect);
            }

            // Ensure the authorization code was issued to the authenticated client.
            var authorizationClientId = authorizationCode.ClientId;
            if (authorizationClientId != authorizationCodeGrantTypeParameter.ClientId)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    string.Format(ErrorDescriptions.TheAuthorizationCodeHasNotBeenIssuedForTheGivenClientId,
                        authorizationCodeGrantTypeParameter.ClientId));
            }

            if (authorizationCode.RedirectUri != authorizationCodeGrantTypeParameter.RedirectUri)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheRedirectionUrlIsNotTheSame);
            }

            // Ensure the authorization code is still valid.
            var authCodeValidity = _simpleIdentityServerConfigurator.GetAuthorizationCodeValidityPeriodInSeconds();
            var expirationDateTime = authorizationCode.CreateDateTime.AddSeconds(authCodeValidity);
            var currentDateTime = DateTime.UtcNow;
            if (currentDateTime > expirationDateTime)
            {
                throw new IdentityServerException(ErrorCodes.InvalidGrant,
                    ErrorDescriptions.TheAuthorizationCodeIsObsolete);
            }

            // Ensure that the redirect_uri parameter value is identical to the redirect_uri parameter value.
            var redirectionUrl = _clientValidator.ValidateRedirectionUrl(authorizationCodeGrantTypeParameter.RedirectUri, client);
            if (redirectionUrl == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectionUrl));
            }

            return authorizationCode;
        }
    }
}
