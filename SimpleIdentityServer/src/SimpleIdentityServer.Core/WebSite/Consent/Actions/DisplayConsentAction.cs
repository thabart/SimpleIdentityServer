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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Extensions;

namespace SimpleIdentityServer.Core.WebSite.Consent.Actions
{
    public interface IDisplayConsentAction
    {
        /// <summary>
        /// Fetch the scopes and client name from the ClientRepository and the parameter
        /// Those informations are used to create the consent screen.
        /// </summary>
        /// <param name="authorizationParameter">Authorization code grant type parameter.</param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="client">Information about the client</param>
        /// <param name="allowedScopes">Allowed scopes</param>
        /// <param name="allowedClaims">Allowed claims</param>
        /// <returns>Action result.</returns>
        ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            out Client client,
            out List<Scope> allowedScopes,
            out List<string> allowedClaims);
    }

    public class DisplayConsentAction : IDisplayConsentAction
    {
        private readonly IScopeRepository _scopeRepository;

        private readonly IClientRepository _clientRepository;

        private readonly IConsentHelper _consentHelper;

        private readonly IGenerateAuthorizationResponse _generateAuthorizationResponse;

        private readonly IParameterParserHelper _parameterParserHelper;

        private readonly IActionResultFactory _actionResultFactory;

        public DisplayConsentAction(
            IScopeRepository scopeRepository,
            IClientRepository clientRepository,
            IConsentHelper consentHelper,
            IGenerateAuthorizationResponse generateAuthorizationResponse,
            IParameterParserHelper parameterParserHelper,
            IActionResultFactory actionResultFactory)
        {
            _scopeRepository = scopeRepository;
            _clientRepository = clientRepository;
            _consentHelper = consentHelper;
            _generateAuthorizationResponse = generateAuthorizationResponse;
            _parameterParserHelper = parameterParserHelper;
            _actionResultFactory = actionResultFactory;
        }

        /// <summary>
        /// Fetch the scopes and client name from the ClientRepository and the parameter
        /// Those informations are used to create the consent screen.
        /// </summary>
        /// <param name="authorizationParameter">Authorization code grant type parameter.</param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="client">Information about the client</param>
        /// <param name="allowedScopes">Allowed scopes</param>
        /// <param name="allowedClaims">Allowed claims</param>
        /// <returns>Action result.</returns>
        public ActionResult Execute(
            AuthorizationParameter authorizationParameter,
            ClaimsPrincipal claimsPrincipal,
            out Client client,
            out List<Scope> allowedScopes,
            out List<string> allowedClaims)
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
            
            allowedClaims = new List<string>();
            allowedScopes = new List<Scope>();
            client = null;

            ActionResult actionResult;
            var subject = claimsPrincipal.GetSubject();
            var assignedConsent = _consentHelper.GetConsentConfirmedByResourceOwner(subject, authorizationParameter);
            // If there's already a consent then redirect to the callback
            if (assignedConsent != null)
            {
                actionResult = _actionResultFactory.CreateAnEmptyActionResultWithRedirectionToCallBackUrl();
                _generateAuthorizationResponse.Execute(actionResult, authorizationParameter, claimsPrincipal);
                var responseMode = authorizationParameter.ResponseMode;
                if (responseMode == ResponseMode.None)
                {
                    var responseTypes = _parameterParserHelper.ParseResponseType(authorizationParameter.ResponseType);
                    var authorizationFlow = GetAuthorizationFlow(responseTypes, authorizationParameter.State);
                    responseMode = GetResponseMode(authorizationFlow);
                }

                actionResult.RedirectInstruction.ResponseMode = responseMode;
                return actionResult;
            }

            // Redirect to the consent screen
            client = _clientRepository.GetClientById(authorizationParameter.ClientId);
            if (client == null)
            {
                throw new IdentityServerExceptionWithState(ErrorCodes.InvalidRequestCode,
                    string.Format(ErrorDescriptions.ClientIsNotValid, authorizationParameter.ClientId),
                    authorizationParameter.State);
            }

            var claimsParameter = authorizationParameter.Claims;
            if (claimsParameter.IsAnyIdentityTokenClaimParameter() ||
                claimsParameter.IsAnyUserInfoClaimParameter())
            {
                allowedClaims = claimsParameter.GetClaimNames();
            }
            else
            {
                allowedScopes = GetScopes(authorizationParameter.Scope)
                    .Where(s => s.IsDisplayedInConsent)
                    .ToList();
            }

            actionResult = _actionResultFactory.CreateAnEmptyActionResultWithOutput();
            return actionResult;
        }

        /// <summary>
        /// Returns a list of scopes from a concatenate list of scopes separated by whitespaces.
        /// </summary>
        /// <param name="concatenateListOfScopes"></param>
        /// <returns>List of scopes</returns>
        private IEnumerable<Scope> GetScopes(string concatenateListOfScopes)
        {
            var result = new List<Scope>();
            var scopeNames = concatenateListOfScopes.Split(' ');
            foreach (var scopeName in scopeNames)
            {
                var scope = _scopeRepository.GetScopeByName(scopeName);
                if (scope != null)
                {
                    result.Add(scope);
                }
            }

            return result;
        }

        #region Private static methods

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

        #endregion
    }
}
