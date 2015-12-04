using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Services;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface ILocalUserAuthenticationAction
    {
        /// <summary>
        /// Authenticate local user account.
        /// 1). Return an error if the user is already authenticated.
        /// 2). Redirect to the index action if the user credentials are not correct
        /// 3). If there's no consent which has already been approved by the resource owner, then redirect the user-agent to the consent screen.
        /// 4). Redirect the user-agent to the callback-url and pass the authorization code as parameter.
        /// Exceptions :
        /// Throw the exception <see cref="IdentityServerAuthenticationException "/> if the user can't be authenticated
        /// </summary>
        /// <param name="localAuthorizationParameter">User's credentials</param>
        /// <param name="parameter">Authorization parameters</param>
        /// <param name="resourceOwnerPrincipal">Resource owner principal</param>
        /// <param name="code">Encrypted & signed authorization parameters</param>
        /// <param name="claims">Returned the claims of the authenticated user</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        ActionResult Execute(
            LocalAuthorizationParameter localAuthorizationParameter,
            AuthorizationParameter parameter,
            ClaimsPrincipal resourceOwnerPrincipal,
            string code,
            out List<Claim> claims);
    }

    public class LocalUserAuthenticationAction : ILocalUserAuthenticationAction
    {
        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IResourceOwnerService _resourceOwnerService;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly IConsentRepository _consentRepository;

        private readonly IActionResultFactory _actionResultFactory;

        public LocalUserAuthenticationAction(
            IParameterParserHelper parameterParserHelper,
            IResourceOwnerService resourceOwnerService,
            IResourceOwnerRepository resourceOwnerRepository,
            IConsentRepository consentRepository,
            IActionResultFactory actionResultFactory)
        {
            _parameterParserHelper = parameterParserHelper;
            _resourceOwnerService = resourceOwnerService;
            _resourceOwnerRepository = resourceOwnerRepository;
            _consentRepository = consentRepository;
            _actionResultFactory = actionResultFactory;
        }

        /// <summary>
        /// Authenticate local user account.
        /// 1). Return an error if the user is already authenticated.
        /// 2). Redirect to the index action if the user credentials are not correct
        /// 3). If there's no consent which has already been approved by the resource owner, then redirect the user-agent to the consent screen.
        /// 4). Redirect the user-agent to the callback-url and pass the authorization code as parameter.
        /// Exceptions :
        /// Throw the exception <see cref="IdentityServerAuthenticationException "/> if the user can't be authenticated
        /// </summary>
        /// <param name="localAuthorizationParameter">User's credentials</param>
        /// <param name="parameter">Authorization parameters</param>
        /// <param name="resourceOwnerPrincipal">Resource owner principal</param>
        /// <param name="code">Encrypted & signed authorization parameters</param>
        /// <param name="claims">Returned the claims of the authenticated user</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        public ActionResult Execute(
            LocalAuthorizationParameter localAuthorizationParameter,
            AuthorizationParameter parameter,
            ClaimsPrincipal resourceOwnerPrincipal,
            string code,
            out List<Claim> claims)
        {
            claims = new List<Claim>();
            var promptParameters = _parameterParserHelper.ParsePromptParameters(parameter.Prompt);
            var resourceOwnerIsAuthenticated = resourceOwnerPrincipal.IsAuthenticated();
            if (resourceOwnerIsAuthenticated && !promptParameters.Contains(PromptParameter.login))
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheUserCannotBeReauthenticated,
                    parameter.State);
            }

            var subject = _resourceOwnerService.Authenticate(localAuthorizationParameter.UserName,
                localAuthorizationParameter.Password);
            if (string.IsNullOrEmpty(subject))
            {
                throw new IdentityServerAuthenticationException("the user credentials are not correct");
            }

            // FamilyName, 
            var resourceOwner = _resourceOwnerRepository.GetBySubject(subject);
            // Add the standard open-id claims.
            claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, resourceOwner.Id));
            if (!string.IsNullOrWhiteSpace(resourceOwner.BirthDate))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate, resourceOwner.BirthDate));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Email))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Email, resourceOwner.Email));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.FamilyName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName, resourceOwner.FamilyName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Gender))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Gender, resourceOwner.Gender));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.GivenName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.GivenName, resourceOwner.GivenName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Locale))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Locale, resourceOwner.Locale));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.MiddleName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName, resourceOwner.MiddleName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Name))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Name, resourceOwner.Name));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.NickName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.NickName, resourceOwner.NickName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.PhoneNumber))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, resourceOwner.PhoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Picture))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Picture, resourceOwner.Picture));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.PreferredUserName))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName, resourceOwner.PreferredUserName));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.Profile))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Profile, resourceOwner.Profile));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.WebSite))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.WebSite, resourceOwner.WebSite));
            }

            if (!string.IsNullOrWhiteSpace(resourceOwner.ZoneInfo))
            {
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo, resourceOwner.ZoneInfo));
            }

            var address = resourceOwner.Address;
            if (address != null)
            {
                var serializedAddress = address.SerializeWithDataContract();
                claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Address, serializedAddress));
            }
            
            claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified, resourceOwner.EmailVerified.ToString(CultureInfo.InvariantCulture)));
            claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, resourceOwner.PhoneNumberVerified.ToString(CultureInfo.InvariantCulture)));
            claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, resourceOwner.UpdatedAt.ToString(CultureInfo.InvariantCulture)));
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant, 
                DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(),
                ClaimValueTypes.Integer));
            
            var requestedScopes = _parameterParserHelper.ParseScopeParameters(parameter.Scope);
            var consents = _consentRepository.GetConsentsForGivenUser(subject);
            if (consents != null && consents.Any())
            {
                var alreadyOneConsentExist = consents.Any(
                    c =>
                        c.Client.ClientId == parameter.ClientId && 
                        c.GrantedScopes != null && c.GrantedScopes.Any() &&
                        c.GrantedScopes.All(s => !s.IsInternal && requestedScopes.Contains(s.Name)));
                if (alreadyOneConsentExist)
                {
                    return _actionResultFactory.CreateAnEmptyActionResultWithNoEffect();
                }
            }

            var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
            result.RedirectInstruction.Action = IdentityServerEndPoints.ConsentIndex;
            result.RedirectInstruction.AddParameter("code", code);
            return result;
        }
    }
}
