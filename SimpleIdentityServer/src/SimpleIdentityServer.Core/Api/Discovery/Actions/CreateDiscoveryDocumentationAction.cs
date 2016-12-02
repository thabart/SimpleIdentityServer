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

using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Core.Api.Discovery.Actions
{
    public interface ICreateDiscoveryDocumentationAction
    {
        DiscoveryInformation Execute();
    }

    public class CreateDiscoveryDocumentationAction : ICreateDiscoveryDocumentationAction
    {
        private readonly IScopeRepository _scopeRepository;
        private readonly IClaimRepository _claimRepository;

        public CreateDiscoveryDocumentationAction(IScopeRepository scopeRepository, IClaimRepository claimRepository)
        {
            _scopeRepository = scopeRepository;
            _claimRepository = claimRepository;
        }

        public DiscoveryInformation Execute()
        {
            var result = new DiscoveryInformation();

            // Returns only the exposed scopes
            var scopes = _scopeRepository.GetAllScopes();
            var scopeSupportedNames = new string[0];
            if (scopes != null ||
                scopes.Any())
            {
                scopeSupportedNames = scopes.Where(s => s.IsExposed).Select(s => s.Name).ToArray();
            }

            var responseTypesSupported = GetSupportedResponseTypes(Constants.Supported.SupportedAuthorizationFlows);

            var grantTypesSupported = GetSupportedGrantTypes();
            var tokenAuthMethodSupported = GetSupportedTokenEndPointAuthMethods();

            result.ClaimsParameterSupported = true;
            result.RequestParameterSupported = true;
            result.RequestUriParameterSupported = true;
            result.RequireRequestUriRegistration = true;
            result.ClaimsSupported = _claimRepository.GetAll().ToArray();
            result.ScopesSupported = scopeSupportedNames;
            result.ResponseTypesSupported = responseTypesSupported;
            result.ResponseModesSupported = Constants.Supported.SupportedResponseModes.ToArray();
            result.GrantTypesSupported = grantTypesSupported;
            result.SubjectTypesSupported = Constants.Supported.SupportedSubjectTypes.ToArray();
            result.TokenEndpointAuthMethodSupported = tokenAuthMethodSupported;
            result.IdTokenSigningAlgValuesSupported = Constants.Supported.SupportedJwsAlgs.ToArray();

            return result;
        }

        private static string[] GetSupportedResponseTypes(ICollection<AuthorizationFlow> authorizationFlows)
        {
            var result = new List<string>();
            foreach (var mapping in Constants.MappingResponseTypesToAuthorizationFlows)
            {
                if (authorizationFlows.Contains(mapping.Value))
                {
                    var record = string.Join(" ", mapping.Key.Select(k => Enum.GetName(typeof (ResponseType), k)));
                    result.Add(record);
                }
            }

            return result.ToArray();
        }

        private static string[] GetSupportedGrantTypes()
        {
            var result = new List<string>();
            foreach (var supportedGrantType in Constants.Supported.SupportedGrantTypes)
            {
                var record = Enum.GetName(typeof (GrantType), supportedGrantType);
                result.Add(record);
            }

            return result.ToArray();
        }

        private static string[] GetSupportedTokenEndPointAuthMethods()
        {
            var result = new List<string>();
            foreach (var supportedAuthMethod in Constants.Supported.SupportedTokenEndPointAuthenticationMethods)
            {
                var record = Enum.GetName(typeof(TokenEndPointAuthenticationMethods), supportedAuthMethod);
                result.Add(record);
            }

            return result.ToArray();
        }
    }
}
