using System;
using System.Net.Http.Headers;
using SimpleIdentityServer.Core.Authenticate;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.JwtToken;
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

        private readonly IGrantedTokenGeneratorHelper _grantedTokenGeneratorHelper;

        private readonly IGrantedTokenRepository _grantedTokenRepository;

        private readonly IAuthenticateClient _authenticateClient;

        private readonly IJwtGenerator _jwtGenerator;

        public GetTokenByAuthorizationCodeGrantTypeAction(
            IClientValidator clientValidator,
            IAuthorizationCodeRepository authorizationCodeRepository,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            IGrantedTokenGeneratorHelper grantedTokenGeneratorHelper,
            IGrantedTokenRepository grantedTokenRepository,
            IAuthenticateClient authenticateClient,
            IJwtGenerator jwtGenerator)
        {
            _clientValidator = clientValidator;
            _authorizationCodeRepository = authorizationCodeRepository;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _grantedTokenGeneratorHelper = grantedTokenGeneratorHelper;
            _grantedTokenRepository = grantedTokenRepository;
            _authenticateClient = authenticateClient;
            _jwtGenerator = jwtGenerator;
        }

        public GrantedToken Execute(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter, 
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var authorizationCode = ValidateParameter(
                authorizationCodeGrantTypeParameter, 
                authenticationHeaderValue);

            // Invalide the authorization code by removing it !
            _authorizationCodeRepository.RemoveAuthorizationCode(authorizationCode.Code);
            var grantedToken = _grantedTokenRepository.GetToken(
                authorizationCode.Scopes,
                authorizationCode.ClientId,
                authorizationCode.IdTokenPayload,
                authorizationCode.UserInfoPayLoad);
            if (grantedToken == null)
            {
                grantedToken = _grantedTokenGeneratorHelper.GenerateToken(
                    authorizationCode.ClientId,
                    authorizationCode.Scopes,
                    authorizationCode.UserInfoPayLoad,
                    authorizationCode.IdTokenPayload);
                _grantedTokenRepository.Insert(grantedToken);
            }

            if (grantedToken.UserInfoPayLoad != null)
            {
                grantedToken.IdToken = GenerateIdToken(
                    grantedToken.UserInfoPayLoad,
                    grantedToken.ClientId);
            }

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
            // Authenticate the client
            var errorMessage = string.Empty;
            var instruction = CreateAuthenticateInstruction(authorizationCodeGrantTypeParameter,
                authenticationHeaderValue);
            var client = _authenticateClient.Authenticate(instruction, out errorMessage);
            if (client == null)
            {
                throw new IdentityServerException(ErrorCodes.InvalidClient,
                    errorMessage);
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
            if (authorizationClientId != client.ClientId)
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

        private AuthenticateInstruction CreateAuthenticateInstruction(
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            AuthenticationHeaderValue authenticationHeaderValue)
        {
            var result = new AuthenticateInstruction
            {
                ClientAssertion = authorizationCodeGrantTypeParameter.ClientAssertion,
                ClientAssertionType = authorizationCodeGrantTypeParameter.ClientAssertionType,
                ClientIdFromHttpRequestBody = authorizationCodeGrantTypeParameter.ClientId,
                ClientSecretFromHttpRequestBody = authorizationCodeGrantTypeParameter.ClientSecret
            };

            if (authenticationHeaderValue != null && !string.IsNullOrWhiteSpace(authenticationHeaderValue.Parameter))
            {
                var parameters = GetParameters(authenticationHeaderValue.Parameter);
                if (parameters != null)
                {
                    result.ClientIdFromAuthorizationHeader = parameters[0];
                    result.ClientSecretFromAuthorizationHeader = parameters[1];
                }
            }

            return result;
        }
        
        /// <summary>
        /// Generate the JWS payload for identity token.
        /// If at least one claim is defined then returns the filtered result
        /// Otherwise returns the default payload based on the scopes.
        /// </summary>
        /// <param name="jwsPayload"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        private string GenerateIdToken(
            JwsPayload jwsPayload,
            string clientId)
        {
            var idToken = _jwtGenerator.Sign(jwsPayload, clientId);
            return _jwtGenerator.Encrypt(idToken, clientId);
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
    }
}
