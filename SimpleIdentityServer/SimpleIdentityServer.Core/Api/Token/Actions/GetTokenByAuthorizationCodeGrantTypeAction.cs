using System;
using System.Linq;
using System.Net.Http.Headers;

using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Jwt.Validators;
using SimpleIdentityServer.Core.Jwt;

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

        private readonly IClientRepository _clientRepository;

        private readonly IJwtClientParameterValidator _jwtClientParameterValidator;

        public GetTokenByAuthorizationCodeGrantTypeAction(
            IClientValidator clientValidator,
            IAuthorizationCodeRepository authorizationCodeRepository,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            ITokenHelper tokenHelper,
            IClientRepository clientRepository,
            IJwtClientParameterValidator jwtClientParameterValidator)
        {
            _clientValidator = clientValidator;
            _authorizationCodeRepository = authorizationCodeRepository;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _tokenHelper = tokenHelper;
            _clientRepository = clientRepository;
            _jwtClientParameterValidator = jwtClientParameterValidator;
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
            // Try to get the client id & client secret from the HTTP BODY or the header.
            var clientId = TryGettingClientId(authorizationCodeGrantTypeParameter,
                authenticationHeaderValue);

            var clientSecretPost = authorizationCodeGrantTypeParameter.ClientSecret;
            var clientSecretBasic = GetClientSecretBasic(authenticationHeaderValue);

            // Authenticate the client
            var isAuthenticated = _clientValidator.ValidateClientIsAuthenticated(
                clientId,
                clientSecretPost,
                clientSecretBasic);

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
            if (authorizationClientId != clientId)
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
            var client = _clientRepository.GetClientById(clientId);
            var redirectionUrl = _clientValidator.ValidateRedirectionUrl(authorizationCodeGrantTypeParameter.RedirectUri, client);
            if (redirectionUrl == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectionUrl));
            }

            return authorizationCode;
        }

        /// <summary>
        /// Try to get the client id from the HTTP body or HTTP header.
        /// </summary>
        /// <param name="authorizationCodeGrantTypeParameter"></param>
        /// <param name="authenticationHeaderValue"></param>
        /// <returns></returns>
        private string TryGettingClientId(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            // Retrieve the client_id from the Jwt bearer.
            if (authorizationCodeGrantTypeParameter.ClientAssertionType 
                == Constants.StandardClientAssertionTypes.JwtBearer)
            {
                if (string.IsNullOrWhiteSpace(authorizationCodeGrantTypeParameter.ClientAssertion))
                {
                    throw new IdentityServerException(ErrorCodes.InvalidClient,
                        ErrorDescriptions.TheClientCannotBeAuthenticated);
                }

                string errorCode;
                string errorDescription;
                var isValid = _jwtClientParameterValidator.Validate(
                    authorizationCodeGrantTypeParameter.ClientAssertion,
                    out errorCode,
                    out errorDescription);
                if (!isValid)
                {
                    throw new IdentityServerException(errorCode, errorDescription);
                }

                var decodedJwt = authorizationCodeGrantTypeParameter.ClientAssertion.Base64Decode();
                var jwsPayload = decodedJwt.DeserializeWithJavascript<JwsPayload>();
                return jwsPayload.GetClaimValue(Constants.StandardResourceOwnerClaimNames.Subject);
            }

            var result = authorizationCodeGrantTypeParameter.ClientId;
            if (string.IsNullOrWhiteSpace(result))
            {
                var authenticationHeaderParameter = authenticationHeaderValue.Parameter;
                var parameters = GetParameters(authenticationHeaderParameter);
                if (!parameters.Any())
                {
                    throw new IdentityServerException(ErrorCodes.InvalidClient,
                        ErrorDescriptions.TheClientCannotBeAuthenticated);
                }

                result = parameters[0];
            }

            return result;
        }
        
        private static string[] GetParameters(string authorizationHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeaderValue))
            {
                return new string[0];
            }

            var decodedParameter = authorizationHeaderValue.Base64Decode();
            return decodedParameter.Split(':');
        }

        private static string GetClientSecretBasic(AuthenticationHeaderValue authorizationHeader)
        {
            if (authorizationHeader == null)
            {
                return string.Empty;
            }

            var parameters = GetParameters(authorizationHeader.Parameter);
            if (!parameters.Any())
            {
                return null;
            }

            return parameters[1];
        }
    }
}
