using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.WebSite.Consent.Actions
{
    public interface IConfirmConsentAction
    {
        ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal);
    }

    public class ConfirmConsentAction : IConfirmConsentAction
    {
        private readonly IConsentRepository _consentRepository;

        private readonly IClientRepository _clientRepository;

        private readonly IScopeRepository _scopeRepository;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IActionResultFactory _actionResultFactory;

        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;

        public ConfirmConsentAction(
            IConsentRepository consentRepository,
            IClientRepository clientRepository,
            IScopeRepository scopeRepository,
            IResourceOwnerRepository resourceOwnerRepository,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory,
            IGenerateAuthorizationResponse generateAuthorizationResponse)
        {
            _consentRepository = consentRepository;
            _clientRepository = clientRepository;
            _scopeRepository = scopeRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
            _generateAuthorizationResponse = generateAuthorizationResponse;
        }

        /// <summary>
        /// This method is executed when the user confirm the consent
        /// 1). If there's already consent confirmed in the past by the resource owner
        /// 1).* then we generate an authorization code and redirects to the callback.
        /// 2). If there's no consent then we insert it and the authorization code is returned
        ///  2°.* to the callback url.
        /// </summary>
        /// <param name="authorizationParameter">Authorization code grant-type</param>
        /// <param name="claimsPrincipal">Resource owner's claims</param>
        /// <returns>Redirects the authorization code to the callback.</returns>
        public ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            var subject = claimsPrincipal.GetSubject();
            var consents = _consentRepository.GetConsentsForGivenUser(subject);
            Models.Consent assignedConsent = null;
            if (consents != null && consents.Any())
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter == null ||
                    (claimsParameter.IdToken == null ||
                     !claimsParameter.IdToken.Any()) &&
                    (claimsParameter.UserInfo == null ||
                     !claimsParameter.UserInfo.Any()))
                {
                    var expectedClaims = GetClaims(claimsParameter);
                    assignedConsent = consents.FirstOrDefault(
                        c =>
                            c.Client.ClientId == authorizationParameter.ClientId &&
                            c.GrantedScopes != null && c.GrantedScopes.Any() &&
                            c.Claims.All(cl => expectedClaims.Contains(cl)));
                }
                else
                {
                    var scopeNames =
                        _parameterParserHelper.ParseScopeParameters(authorizationParameter.Scope);
                    assignedConsent = consents.FirstOrDefault(
                        c =>
                            c.Client.ClientId == authorizationParameter.ClientId &&
                            c.GrantedScopes != null && c.GrantedScopes.Any() &&
                            c.GrantedScopes.All(s => scopeNames.Contains(s.Name)));
                }
            }

            if (assignedConsent == null)
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter == null ||
                    (claimsParameter.IdToken == null ||
                     !claimsParameter.IdToken.Any()) &&
                    (claimsParameter.UserInfo == null ||
                     !claimsParameter.UserInfo.Any()))
                {
                    // A consent can be given to a set of scopes
                    assignedConsent = new Models.Consent
                    {
                        Client = _clientRepository.GetClientById(authorizationParameter.ClientId),
                        GrantedScopes = GetScopes(authorizationParameter.Scope),
                        ResourceOwner = _resourceOwnerRepository.GetBySubject(subject)
                    };
                }
                else
                {
                    // A consent can be given to a set of claims
                    assignedConsent = new Models.Consent
                    {
                        Client = _clientRepository.GetClientById(authorizationParameter.ClientId),
                        ResourceOwner = _resourceOwnerRepository.GetBySubject(subject),
                        Claims = GetClaims(claimsParameter)
                    };
                }

                // A consent can be given to a set of claims

                _consentRepository.InsertConsent(assignedConsent);
            }

            var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
            _generateAuthorizationResponse.Execute(result, authorizationParameter, claimsPrincipal);
            return result;
        }
        
        /// <summary>
        /// Returns a list of scopes from a concatenate list of scopes separated by whitespaces.
        /// </summary>
        /// <param name="concatenateListOfScopes"></param>
        /// <returns>List of scopes</returns>
        private List<Scope> GetScopes(string concatenateListOfScopes)
        {
            var result = new List<Scope>();
            var scopeNames = _parameterParserHelper.ParseScopeParameters(concatenateListOfScopes);
            foreach (var scopeName in scopeNames)
            {
                var scope = _scopeRepository.GetScopeByName(scopeName);
                result.Add(scope);
            }

            return result;
        }

        /// <summary>
        /// Returns a list of claims.
        /// </summary>
        /// <param name="claimsParameter"></param>
        /// <returns></returns>
        private List<string> GetClaims(ClaimsParameter claimsParameter)
        {
            var result = new List<string>();
            if (claimsParameter.IdToken != null &&
                !claimsParameter.IdToken.Any())
            {
                result.AddRange(claimsParameter.IdToken.Select(s => s.Name));
            }

            if (claimsParameter.UserInfo != null &&
                !claimsParameter.UserInfo.Any())
            {
                result.AddRange(claimsParameter.UserInfo.Select(s => s.Name));
            }

            return result;
        } 
    }
}
