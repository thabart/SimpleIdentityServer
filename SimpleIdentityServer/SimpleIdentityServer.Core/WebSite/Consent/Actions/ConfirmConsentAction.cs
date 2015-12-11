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
using System.Collections.Generic;
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

        private readonly IConsentHelper _consentHelper;

        public ConfirmConsentAction(
            IConsentRepository consentRepository,
            IClientRepository clientRepository,
            IScopeRepository scopeRepository,
            IResourceOwnerRepository resourceOwnerRepository,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IConsentHelper consentHelper)
        {
            _consentRepository = consentRepository;
            _clientRepository = clientRepository;
            _scopeRepository = scopeRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _consentHelper = consentHelper;
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
            Models.Consent assignedConsent = _consentHelper.GetConsentConfirmedByResourceOwner(subject, authorizationParameter);
            // Insert a new consent.
            if (assignedConsent == null)
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter.IsAnyIdentityTokenClaimParameter() ||
                    claimsParameter.IsAnyUserInfoClaimParameter())
                {
                    // A consent can be given to a set of claims
                    assignedConsent = new Models.Consent
                    {
                        Client = _clientRepository.GetClientById(authorizationParameter.ClientId),
                        ResourceOwner = _resourceOwnerRepository.GetBySubject(subject),
                        Claims = claimsParameter.GetClaimNames()
                    };
                }
                else
                {
                    // A consent can be given to a set of scopes
                    assignedConsent = new Models.Consent
                    {
                        Client = _clientRepository.GetClientById(authorizationParameter.ClientId),
                        GrantedScopes = GetScopes(authorizationParameter.Scope),
                        ResourceOwner = _resourceOwnerRepository.GetBySubject(subject)
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
    }
}
