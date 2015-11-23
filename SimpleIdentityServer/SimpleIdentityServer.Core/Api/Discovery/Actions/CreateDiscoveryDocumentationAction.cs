using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;

namespace SimpleIdentityServer.Core.Api.Discovery.Actions
{
    public interface ICreateDiscoveryDocumentationAction
    {
        DiscoveryInformation Execute();
    }

    public class CreateDiscoveryDocumentationAction : ICreateDiscoveryDocumentationAction
    {
        private readonly IScopeRepository _scopeRepository;

        public CreateDiscoveryDocumentationAction(IScopeRepository scopeRepository)
        {
            _scopeRepository = scopeRepository;
        }

        public DiscoveryInformation Execute()
        {
            var result = new DiscoveryInformation();

            // Returns only the exposed scopes
            var scopes = _scopeRepository.GetAllScopes();
            var scopeSupportedNames = scopes.Where(s => s.IsExposed).Select(s => s.Name).ToArray();

            var responseTypesSupported = GetSupportedResponseTypes(Constants.Supported.SupportedAuthorizationFlows);

            var grantTypesSupported = GetSupportedGrantTypes();
            var tokenAuthMethodSupported = GetSupportedTokenEndPointAuthMethods();

            result.ClaimsParameterSupported = false;
            result.RequestParameterSupported = false;
            result.RequestUriParameterSupported = true;
            result.RequireRequestUriRegistration = true;
            result.ClaimTypesSupported = Constants.Supported.SupportedClaims.ToArray();
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
