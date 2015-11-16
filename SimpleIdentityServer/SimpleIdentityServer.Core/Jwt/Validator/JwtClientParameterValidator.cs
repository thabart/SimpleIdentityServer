using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Linq;

namespace SimpleIdentityServer.Core.Jwt.Validators
{
    public interface IJwtClientParameterValidator
    {
        /// <summary>
        /// Validate the JSON Web Token (JWT) and returns the errors
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns></returns>
        bool Validate(
            string jwt,
            out string errorCode,
            out string errorDescription);
    }

    public class JwtClientParameterValidator : IJwtClientParameterValidator
    {
        private readonly IClientValidator _clientValidator;

        private readonly ISimpleIdentityServerConfigurator _configurator;

        private readonly IJwtBearerClientRepository _jwtBearerClientRepository;

        public JwtClientParameterValidator(
            IClientValidator clientValidator,
            ISimpleIdentityServerConfigurator configurator,
            IJwtBearerClientRepository jwtBearerClientRepository)
        {
            _clientValidator = clientValidator;
            _configurator = configurator;
            _jwtBearerClientRepository = jwtBearerClientRepository;
        }

        /// <summary>
        /// Validate the JWT token and returns the issues.
        /// </summary>
        /// <param name="jwt"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorDescription"></param>
        /// <returns></returns>
        public bool Validate(
            string jwt, 
            out string errorCode, 
            out string errorDescription)
        {
            var result = true;
            errorCode = string.Empty;
            errorDescription = string.Empty;

            var decodedJwt = jwt.Base64Decode();
            var jwsPayload = decodedJwt.DeserializeWithJavascript<JwsPayload>();
            var subject = jwsPayload.GetClaimValue(Constants.StandardResourceOwnerClaimNames.Subject);
            var issuer = jwsPayload.Issuer;
            var audiences = jwsPayload.Audiences;
            var expirationTime = jwsPayload.ExpirationTime;
            var issuedAt = jwsPayload.Iat;
            var jti = jwsPayload.Jti;

            // Check if all the mandatories parameters are present
            if (string.IsNullOrWhiteSpace(subject) 
                || string.IsNullOrWhiteSpace(issuer) 
                || audiences == null 
                || !audiences.Any()
                || expirationTime == default(double)
                || subject != issuer
                || string.IsNullOrWhiteSpace(jti)) 
            {
                result = false;
                errorDescription = string.Format(ErrorDescriptions.ParameterIsNotCorrect, "client_assertion");
            }

            // Check if the client exist
            var client = _clientValidator.ValidateClientExist(subject);
            if (client == null)
            {
                result = false;
                errorDescription = ErrorDescriptions.ClientIsNotValid;
            }

            // Check if the identity server belong to the audience list.
            var issuerName = _configurator.GetIssuerName();
            if (!audiences.Contains(issuerName))
            {
                result = false;
                errorDescription = ErrorDescriptions.TheIdServerIsNotPresentInTheAudience;
            }
            
            // Check if the JTI already exists in the cache.
            if (_jwtBearerClientRepository.Exist(jti))
            {
                result = false;
                errorDescription = ErrorDescriptions.TheJwtTokenHasAlreadyBeenUsed;
            }

            // Check the expiration.
            var expirationDateTime = expirationTime.ConvertFromUnixTimestamp();
            var todayDateTime = DateTime.UtcNow;
            if (expirationDateTime < todayDateTime)
            {
                result = false;
                errorDescription = ErrorDescriptions.TheJwtTokenIsExpired;
            }
            
            if (!result)
            {
                errorCode = ErrorCodes.InvalidGrant;
            }

            _jwtBearerClientRepository.Insert(jti);
            return result;
        }
    }
}
