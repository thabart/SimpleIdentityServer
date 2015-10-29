using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
            AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            ClaimsPrincipal claimsPrincipal);
    }

    public class ConfirmConsentAction : IConfirmConsentAction
    {
        private readonly IConsentRepository _consentRepository;

        private readonly IClientRepository _clientRepository;

        private readonly IScopeRepository _scopeRepository;

        private readonly IAuthorizationCodeRepository _authorizationCodeRepository;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IActionResultFactory _actionResultFactory;

        public ConfirmConsentAction(
            IConsentRepository consentRepository,
            IClientRepository clientRepository,
            IScopeRepository scopeRepository,
            IResourceOwnerRepository resourceOwnerRepository,
            IAuthorizationCodeRepository authorizationCodeRepository,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory)
        {
            _consentRepository = consentRepository;
            _clientRepository = clientRepository;
            _scopeRepository = scopeRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
            _authorizationCodeRepository = authorizationCodeRepository;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
        }

        /// <summary>
        /// This method is executed when the user confirm the consent
        /// 1). If there's already consent confirmed in the past by the resource owner
        /// 1).* then we generate an authorization code and redirects to the callback.
        /// 2). If there's no consent then we insert it and the authorization code is returned
        ///  2°.* to the callback url.
        /// </summary>
        /// <param name="authorizationCodeGrantTypeParameter">Authorization code grant-type</param>
        /// <param name="claimsPrincipal">Resource owner's claims</param>
        /// <returns>Redirects the authorization code to the callback.</returns>
        public ActionResult Execute(AuthorizationCodeGrantTypeParameter authorizationCodeGrantTypeParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            var subject = claimsPrincipal.GetSubject();
            var consents = _consentRepository.GetConsentsForGivenUser(subject);
            var scopeNames = _parameterParserHelper.ParseScopeParameters(authorizationCodeGrantTypeParameter.Scope);
            var assignedConsent = consents.FirstOrDefault(
                    c =>
                        c.Client.ClientId == authorizationCodeGrantTypeParameter.ClientId && c.GrantedScopes.All(s => !s.IsInternal && scopeNames.Contains(s.Name)));
            if (assignedConsent == null)
            {
                assignedConsent = new Models.Consent
                {
                    Client = _clientRepository.GetClientById(authorizationCodeGrantTypeParameter.ClientId),
                    GrantedScopes = GetScopes(authorizationCodeGrantTypeParameter.Scope),
                    ResourceOwner = _resourceOwnerRepository.GetBySubject(subject)
                };
            }

            var authorizationCode = new AuthorizationCode
            {
                CreateDateTime = DateTime.UtcNow,
                Consent = assignedConsent,
                Value = Guid.NewGuid().ToString()
            };
            _authorizationCodeRepository.AddAuthorizationCode(authorizationCode);
            var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirection();
            result.RedirectInstruction.Action = IdentityServerEndPoints.CallBackUrl;
            result.RedirectInstruction.AddParameter("code", authorizationCode.Value);
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
    }
}
