using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Practices.ObjectBuilder2;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;

namespace SimpleIdentityServer.Core.Jwt.Generator
{
    public interface IJwtGenerator
    {
        JwtClaims GenerateJwtClaims(
            ClaimsPrincipal claimPrincipal,
            AuthorizationParameter authorizationParameter);
    }

    public class JwtGenerator : IJwtGenerator
    {
        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        private readonly IClientRepository _clientRepository;

        private readonly IClientValidator _clientValidator;

        public JwtGenerator(
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator,
            IClientRepository clientRepository,
            IClientValidator clientValidator)
        {
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
        }

        public JwtClaims GenerateJwtClaims(
            ClaimsPrincipal claimPrincipal,
            AuthorizationParameter authorizationParameter)
        {
            // Get the issuer from the configuration.
            var issuerName = _simpleIdentityServerConfigurator.GetIssuerName();
            // Audience can be used to disambiguate the intended recipients.
            // Fill-in the list with the list of client_id suspected to parse the JWT tokens.
            var audiences = new List<string>
            {
                authorizationParameter.ClientId
            };

            var clients = _clientRepository.GetAll();
            clients.ForEach(client =>
            {
                var isClientSupportIdTokenResponseType =
                    _clientValidator.ValidateResponseType(ResponseType.id_token, client);
                var isClientSupportImplicitGrantTypeFlow =
                    _clientValidator.ValidateGrantType(GrantType.@implicit, client);
                if (isClientSupportIdTokenResponseType && isClientSupportImplicitGrantTypeFlow)
                {
                    audiences.Add(client.ClientId);
                }
            });

            // Calculate the expiration datetime
            var currentDateTime = DateTime.Now;
            var expiredDateTime = currentDateTime.AddSeconds(_simpleIdentityServerConfigurator.GetTokenValidityPeriodInSeconds());
            var expirationInSeconds = expiredDateTime.ConvertToUnixTimestamp();
            // Calculate the time in seconds which the JWT was issued.
            var iatInSeconds = currentDateTime.ConvertToUnixTimestamp();
            // Populate the claims
            var claims = new List<Claim>
            {
                new Claim(Constants.StandardResourceOwnerClaimNames.Subject, claimPrincipal.GetSubject())
            };
            
            var result = new JwtClaims
            {
                iss = issuerName,
                aud = audiences.ToArray(),
                exp = expirationInSeconds,
                iat = iatInSeconds,
                Claims = claims
            };

            // If the max_age request is made or when auth_time is requesed as an Essential claim then we calculate the auth_time
            // The auth_time corresponds to the time when the End-User authentication occured. 
            // Its value is a JSON number representing the number of seconds from 1970-01-01
            if (authorizationParameter.MaxAge != 0.0D)
            {
                var authenticationInstant = claimPrincipal.Claims.SingleOrDefault(c => c.Type == ClaimTypes.AuthenticationInstant);
                if (authenticationInstant != null)
                {
                    result.auth_time = double.Parse(authenticationInstant.Value);
                }
            }

            return result;
        }
    }
}
