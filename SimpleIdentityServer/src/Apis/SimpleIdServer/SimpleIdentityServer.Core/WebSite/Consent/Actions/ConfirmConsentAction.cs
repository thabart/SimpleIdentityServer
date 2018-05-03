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
using System.Linq;
using System.Security.Claims;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Logging;
using System;
using SimpleIdentityServer.Core.Services;
using System.Threading.Tasks;
using SimpleIdentityServer.Core.Common.Repositories;

namespace SimpleIdentityServer.Core.WebSite.Consent.Actions
{
    public interface IConfirmConsentAction
    {
        Task<ActionResult> Execute(AuthorizationParameter authorizationParameter, ClaimsPrincipal claimsPrincipal);
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
        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public ConfirmConsentAction(
            IConsentRepository consentRepository,
            IClientRepository clientRepository,
            IScopeRepository scopeRepository,
            IResourceOwnerRepository resourceOwnerRepository,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IConsentHelper consentHelper,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _consentRepository = consentRepository;
            _clientRepository = clientRepository;
            _scopeRepository = scopeRepository;
            _resourceOwnerRepository = resourceOwnerRepository;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _consentHelper = consentHelper;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
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
        public async Task<ActionResult> Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal)
        {
            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            if (claimsPrincipal == null ||
                claimsPrincipal.Identity == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var client = await _clientRepository.GetClientByIdAsync(authorizationParameter.ClientId);
            if (client == null)
            {
                throw new InvalidOperationException(string.Format("the client id {0} doesn't exist",
                    authorizationParameter.ClientId));
            }

            var subject = claimsPrincipal.GetSubject();
            Common.Models.Consent assignedConsent = await _consentHelper.GetConfirmedConsentsAsync(subject, authorizationParameter);
            // Insert a new consent.
            if (assignedConsent == null)
            {
                var claimsParameter = authorizationParameter.Claims;
                if (claimsParameter.IsAnyIdentityTokenClaimParameter() ||
                    claimsParameter.IsAnyUserInfoClaimParameter())
                {
                    // A consent can be given to a set of claims
                    assignedConsent = new Common.Models.Consent
                    {
                        Client = client,
                        ResourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(subject),
                        Claims = claimsParameter.GetClaimNames()
                    };
                }
                else
                {
                    // A consent can be given to a set of scopes
                    assignedConsent = new Common.Models.Consent
                    {
                        Client = client,
                        GrantedScopes = (await GetScopes(authorizationParameter.Scope)).ToList(),
                        ResourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(subject),
                    };
                }

                // A consent can be given to a set of claims
                await _consentRepository.InsertAsync(assignedConsent);

                _simpleIdentityServerEventSource.GiveConsent(subject,
                    authorizationParameter.ClientId,
                    assignedConsent.Id);
            }

            var result = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
            await _generateAuthorizationResponse.ExecuteAsync(result, authorizationParameter, claimsPrincipal, client);

            // If redirect to the callback and the responde mode has not been set.
            if (result.Type == TypeActionResult.RedirectToCallBackUrl)
            {
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseTypes(authorizationParameter.ResponseType);
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                result.RedirectInstruction.ResponseMode = responseMode;
            }

            return result;
        }
        
        /// <summary>
        /// Returns a list of scopes from a concatenate list of scopes separated by whitespaces.
        /// </summary>
        /// <param name="concatenateListOfScopes"></param>
        /// <returns>List of scopes</returns>
        private async Task<ICollection<Scope>> GetScopes(string concatenateListOfScopes)
        {
            var result = new List<Scope>();
            var scopeNames = _parameterParserHelper.ParseScopes(concatenateListOfScopes);
            return await _scopeRepository.SearchByNamesAsync(scopeNames);
        }

        private static AuthorizationFlow GetAuthorizationFlow(ICollection<ResponseType> responseTypes, string state)
        {
            if (responseTypes == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            var record = Constants.MappingResponseTypesToAuthorizationFlows.Keys
                .SingleOrDefault(k => k.Count == responseTypes.Count && k.All(key => responseTypes.Contains(key)));
            if (record == null)
            {
                throw new IdentityServerExceptionWithState(
                    ErrorCodes.InvalidRequestCode,
                    ErrorDescriptions.TheAuthorizationFlowIsNotSupported,
                    state);
            }

            return Constants.MappingResponseTypesToAuthorizationFlows[record];
        }

        private static ResponseMode GetResponseMode(AuthorizationFlow authorizationFlow)
        {
            return Constants.MappingAuthorizationFlowAndResponseModes[authorizationFlow];
        }
    }
}
