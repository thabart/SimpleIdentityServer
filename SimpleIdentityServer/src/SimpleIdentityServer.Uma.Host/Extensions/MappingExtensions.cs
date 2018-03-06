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

using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Parameters;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using DomainResponse = SimpleIdentityServer.Uma.Core.Responses;

namespace SimpleIdentityServer.Uma.Host.Extensions
{
    internal static class MappingExtensions
    {
        #region UMA

        public static AddResouceSetParameter ToParameter(this PostResourceSet postResourceSet)
        {
            return new AddResouceSetParameter
            {
                IconUri = postResourceSet.IconUri,
                Name = postResourceSet.Name,
                Scopes = postResourceSet.Scopes,
                Type = postResourceSet.Type,
                Uri = postResourceSet.Uri
            };
        }

        public static UpdateResourceSetParameter ToParameter(this PutResourceSet putResourceSet)
        {
            return new UpdateResourceSetParameter
            {
                Id = putResourceSet.Id,
                Name = putResourceSet.Name,
                IconUri = putResourceSet.IconUri,
                Scopes = putResourceSet.Scopes,
                Type = putResourceSet.Type,
                Uri = putResourceSet.Uri
            };
        }

        public static AddScopeParameter ToParameter(this PostScope postScope)
        {
            return new AddScopeParameter
            {
                Id = postScope.Id,
                Name = postScope.Name,
                IconUri = postScope.IconUri
            };
        }

        public static UpdateScopeParameter ToParameter(this PutScope putScope)
        {
            return new UpdateScopeParameter
            {
                Id = putScope.Id,
                Name = putScope.Name,
                IconUri = putScope.IconUri
            };
        }

        public static AddPermissionParameter ToParameter(this PostPermission postPermission)
        {
            return new AddPermissionParameter
            {
                ResourceSetId = postPermission.ResourceSetId,
                Scopes = postPermission.Scopes
            };
        }

        public static GetAuthorizationActionParameter ToParameter(this PostAuthorization postAuthorization)
        {
            var tokens = new List<ClaimTokenParameter>();
            if (postAuthorization.ClaimTokens != null &&
                postAuthorization.ClaimTokens.Any())
            {
                tokens = postAuthorization.ClaimTokens.Select(ct => ct.ToParameter()).ToList();
            }

            return new GetAuthorizationActionParameter
            {
                Rpt = postAuthorization.Rpt,
                TicketId = postAuthorization.TicketId,
                ClaimTokenParameters = tokens
            };
        }

        public static ClaimTokenParameter ToParameter(this PostClaimToken postClaimToken)
        {
            return new ClaimTokenParameter
            {
                Format = postClaimToken.Format,
                Token = postClaimToken.Token
            };
        }

        public static AddPolicyParameter ToParameter(this PostPolicy postPolicy)
        {
            var rules = postPolicy.Rules == null ? new List<AddPolicyRuleParameter>()
                : postPolicy.Rules.Select(r => r.ToParameter()).ToList();
            return new AddPolicyParameter
            {
                Rules = rules,
                ResourceSetIds = postPolicy.ResourceSetIds
            };
        }

        public static AddPolicyRuleParameter ToParameter(this PostPolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<AddClaimParameter>()
                : policyRule.Claims.Select(p => p.ToParameter()).ToList();
            return new AddPolicyRuleParameter
            {
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script
            };
        }

        public static AddClaimParameter ToParameter(this PostClaim postClaim)
        {
            return new AddClaimParameter
            {
                Type = postClaim.Type,
                Value = postClaim.Value
            };
        }

        public static UpdatePolicyParameter ToParameter(this PutPolicy putPolicy)
        {
            var rules = putPolicy.Rules == null ? new List<UpdatePolicyRuleParameter>()
                : putPolicy.Rules.Select(r => r.ToParameter()).ToList();
            return new UpdatePolicyParameter
            {
                PolicyId = putPolicy.PolicyId,
                Rules = rules
            };
        }

        public static UpdatePolicyRuleParameter ToParameter(this PutPolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<AddClaimParameter>()
                : policyRule.Claims.Select(p => p.ToParameter()).ToList();
            return new UpdatePolicyRuleParameter
            {
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                Id = policyRule.Id,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script
            };
        }

        public static ResourceSetResponse ToResponse(this ResourceSet resourceSet)
        {
            return new ResourceSetResponse
            {
                Id = resourceSet.Id,
                IconUri = resourceSet.IconUri,
                Name = resourceSet.Name,
                Scopes = resourceSet.Scopes,
                Type = resourceSet.Type,
                Uri = resourceSet.Uri 
            };
        }

        public static PolicyResponse ToResponse(this Policy policy)
        {
            var rules = policy.Rules == null ? new List<PolicyRuleResponse>()
                : policy.Rules.Select(p => p.ToResponse()).ToList();
            return new PolicyResponse
            {
                Id = policy.Id,
                ResourceSetIds = policy.ResourceSetIds,
                Rules = rules
            };
        }

        public static PolicyRuleResponse ToResponse(this PolicyRule policyRule)
        {
            var claims = policyRule.Claims == null ? new List<PostClaim>()
                : policyRule.Claims.Select(p => p.ToResponse()).ToList();
            return new PolicyRuleResponse
            {
                Id = policyRule.Id,
                Claims = claims,
                ClientIdsAllowed = policyRule.ClientIdsAllowed,
                IsResourceOwnerConsentNeeded = policyRule.IsResourceOwnerConsentNeeded,
                Scopes = policyRule.Scopes,
                Script = policyRule.Script
            };
        }

        public static PostClaim ToResponse(this Claim claim)
        {
            return new PostClaim
            {
                Type = claim.Type,
                Value = claim.Value
            };
        }

        public static ConfigurationResponse ToResponse(this DomainResponse.ConfigurationResponse configuration)
        {
            return new ConfigurationResponse
            {
                ClaimTokenProfilesSupported = configuration.ClaimTokenProfilesSupported,
                IntrospectionEndpoint = configuration.IntrospectionEndpoint,
                Issuer = configuration.Issuer,
                PermissionEndpoint = configuration.PermissionEndpoint,
                AuthorizationEndpoint = configuration.AuthorizationEndpoint,
                ClaimsInteractionEndpoint = configuration.ClaimsInteractionEndpoint,
                GrantTypesSupported = configuration.GrantTypesSupported,
                JwksUri = configuration.JwksUri,
                RegistrationEndpoint = configuration.RegistrationEndpoint,
                ResourceRegistrationEndpoint = configuration.ResourceRegistrationEndpoint,
                ResponseTypesSupported = configuration.ResponseTypesSupported,
                RevocationEndpoint = configuration.RevocationEndpoint,
                PoliciesEndpoint = configuration.PoliciesEndpoint,
                ScopesSupported = configuration.ScopesSupported,
                TokenEndpoint = configuration.TokenEndpoint,
                TokenEndpointAuthMethodsSupported = configuration.TokenEndpointAuthMethodsSupported,
                TokenEndpointAuthSigningAlgValuesSupported = configuration.TokenEndpointAuthSigningAlgValuesSupported,
                UiLocalesSupported = configuration.UiLocalesSupported,
                UmaProfilesSupported = configuration.UmaProfilesSupported
            };
        }

        #endregion

        #region OAUTH2.0

        public static RegistrationParameter ToParameter(this ClientResponse clientResponse)
        {
            var responseTypes = new List<ResponseType>();
            var redirectUris = clientResponse.redirect_uris == null
                ? new List<string>()
                : clientResponse.redirect_uris.ToList();
            var grantTypes = new List<GrantType>();
            ApplicationTypes? applicationType = null;
            if (clientResponse.response_types != null &&
                clientResponse.response_types.Any())
            {
                foreach (var responseType in clientResponse.response_types)
                {
                    var responseTypeSplitted = responseType.Split(' ');
                    foreach (var response in responseTypeSplitted)
                    {
                        ResponseType responseTypeEnum;
                        if (Enum.TryParse(response, out responseTypeEnum) &&
                            !responseTypes.Contains(responseTypeEnum))
                        {
                            responseTypes.Add(responseTypeEnum);
                        }
                    }
                }
            }

            if (clientResponse.grant_types != null &&
                clientResponse.grant_types.Any())
            {
                foreach (var grantType in clientResponse.grant_types)
                {
                    GrantType grantTypeEnum;
                    if (Enum.TryParse(grantType, out grantTypeEnum))
                    {
                        grantTypes.Add(grantTypeEnum);
                    }
                }
            }

            ApplicationTypes appTypeEnum;
            if (Enum.TryParse(clientResponse.application_type, out appTypeEnum))
            {
                applicationType = appTypeEnum;
            }

            return new RegistrationParameter
            {
                ApplicationType = applicationType,
                ClientName = clientResponse.client_name,
                ClientUri = clientResponse.client_uri,
                Contacts = clientResponse.contacts == null ? new List<string>() : clientResponse.contacts.ToList(),
                DefaultAcrValues = clientResponse.default_acr_values,
                DefaultMaxAge = clientResponse.default_max_age,
                GrantTypes = grantTypes,
                IdTokenEncryptedResponseAlg = clientResponse.id_token_encrypted_response_alg,
                IdTokenEncryptedResponseEnc = clientResponse.id_token_encrypted_response_enc,
                IdTokenSignedResponseAlg = clientResponse.id_token_signed_response_alg,
                InitiateLoginUri = clientResponse.initiate_login_uri,
                Jwks = clientResponse.jwks,
                JwksUri = clientResponse.jwks_uri,
                LogoUri = clientResponse.logo_uri,
                PolicyUri = clientResponse.policy_uri,
                RedirectUris = redirectUris,
                RequestObjectEncryptionAlg = clientResponse.request_object_encryption_alg,
                RequestObjectEncryptionEnc = clientResponse.request_object_encryption_enc,
                RequestObjectSigningAlg = clientResponse.request_object_signing_alg,
                RequestUris = clientResponse.request_uris,
                RequireAuthTime = clientResponse.require_auth_time,
                ResponseTypes = responseTypes,
                SectorIdentifierUri = clientResponse.sector_identifier_uri,
                SubjectType = clientResponse.subject_type,
                TokenEndPointAuthMethod = clientResponse.token_endpoint_auth_method,
                TokenEndPointAuthSigningAlg = clientResponse.token_endpoint_auth_signing_alg,
                TosUri = clientResponse.tos_uri,
                UserInfoEncryptedResponseAlg = clientResponse.userinfo_encrypted_response_alg,
                UserInfoEncryptedResponseEnc = clientResponse.userinfo_encrypted_response_enc,
                UserInfoSignedResponseAlg = clientResponse.userinfo_signed_response_alg,
                ScimProfile = clientResponse.scim_profile
            };
        }


        public static Introspection ToDto(this IntrospectionResult introspectionResult)
        {
            return new Introspection
            {
                Active = introspectionResult.Active,
                Audience = introspectionResult.Audience,
                ClientId = introspectionResult.ClientId,
                Expiration = introspectionResult.Expiration,
                IssuedAt = introspectionResult.IssuedAt,
                Issuer = introspectionResult.Issuer,
                Jti = introspectionResult.Jti,
                Nbf = introspectionResult.Nbf,
                Scope = introspectionResult.Scope.Split(' ').ToList(),
                Subject = introspectionResult.Subject,
                TokenType = introspectionResult.TokenType,
                UserName = introspectionResult.UserName
            };
        }

        public static ResourceOwnerGrantTypeParameter ToResourceOwnerGrantTypeParameter(this TokenRequest request)
        {
            return new ResourceOwnerGrantTypeParameter
            {
                UserName = request.Username,
                Password = request.Password,
                Scope = request.Scope,
                ClientId = request.ClientId,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientSecret = request.ClientSecret
            };
        }

        public static AuthorizationCodeGrantTypeParameter ToAuthorizationCodeGrantTypeParameter(this TokenRequest request)
        {
            return new AuthorizationCodeGrantTypeParameter
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Code = request.Code,
                RedirectUri = request.RedirectUri,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                CodeVerifier = request.CodeVerifier
            };
        }

        public static RefreshTokenGrantTypeParameter ToRefreshTokenGrantTypeParameter(this TokenRequest request)
        {
            return new RefreshTokenGrantTypeParameter
            {
                RefreshToken = request.RefreshToken
            };
        }

        public static ClientCredentialsGrantTypeParameter ToClientCredentialsGrantTypeParameter(this TokenRequest request)
        {
            return new ClientCredentialsGrantTypeParameter
            {
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                Scope = request.Scope
            };
        }

        public static GetTokenViaTicketIdParameter ToTokenIdGrantTypeParameter(this TokenRequest request)
        {
            return new GetTokenViaTicketIdParameter
            {
                ClaimToken = request.ClaimToken,
                ClaimTokenFormat = request.ClaimTokenFormat,
                ClientId = request.ClientId,
                ClientAssertion = request.ClientAssertion,
                ClientAssertionType = request.ClientAssertionType,
                ClientSecret = request.ClientSecret,
                Pct = request.Pct,
                Rpt = request.Rpt,
                Ticket = request.Ticket
            };
        }

        public static TokenResponse ToDto(this GrantedToken grantedToken)
        {
            if (grantedToken == null)
            {
                throw new ArgumentNullException(nameof(grantedToken));
            }

            return new TokenResponse
            {
                AccessToken = grantedToken.AccessToken,
                IdToken = grantedToken.IdToken,
                ExpiresIn = grantedToken.ExpiresIn,
                RefreshToken = grantedToken.RefreshToken,
                TokenType = grantedToken.TokenType,
                Scope = grantedToken.Scope.Split(' ').ToList()
            };
        }
        
        public static IntrospectionParameter ToParameter(this IntrospectionRequest viewModel)
        {
            return new IntrospectionParameter
            {
                ClientAssertion = viewModel.ClientAssertion,
                ClientAssertionType = viewModel.ClientAssertionType,
                ClientId = viewModel.ClientId,
                ClientSecret = viewModel.ClientSecret,
                Token = viewModel.Token,
                TokenTypeHint = viewModel.TokenTypeHint
            };
        }

        public static RevokeTokenParameter ToParameter(this RevocationRequest revocationRequest)
        {
            return new RevokeTokenParameter
            {
                ClientAssertion = revocationRequest.ClientAssertion,
                ClientAssertionType = revocationRequest.ClientAssertionType,
                ClientId = revocationRequest.ClientId,
                ClientSecret = revocationRequest.ClientSecret,
                Token = revocationRequest.Token,
                TokenTypeHint = revocationRequest.TokenTypeHint
            };
        }

        #endregion
    }
}
