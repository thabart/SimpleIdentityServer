using System.Collections.Generic;
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Core
{
    public static class Constants
    {
        #region Standard definitions
        
        // Open-Id Provider Authentication Policy Extension 1.0
        public static class StandardArcParameterNames
        {
            public static string OpenIdNsPage = "openid.ns.pape";

            public static string OpenIdMaxAuthAge = "openid.pape.max_auth_age";

            public static string OpenIdAuthPolicies = "openid.pape.preferred_auth_policies";

            // Namespace for the custom Assurance Level
            public static string OpenIdCustomAuthLevel = "openid.pape.auth_level.ns";
            
            public static string OpenIdPreferredCustomAuthLevel = "openid.pape.preferred_auth_levels";
        }

        public static class StandardClientAssertionTypes
        {
            public static string JwtBearer = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";
        }

        // Standard authentication policies.
        // They are coming from the RFC : http://openid.net/specs/openid-provider-authentication-policy-extension-1_0.html
        public static class StandardAuthenticationPolicies
        {
            public static string OpenIdPhishingResistant = "http://schemas.openid.net/pape/policies/2007/06/phishing-resistant";

            // provides more than one authentication factor for example password + software token
            public static string OpenIdMultiFactorAuth = "http://schemas.openid.net/pape/policies/2007/06/multi-factor";

            // provides more than one authentication factor with at least one physical factor
            public static string OpenIdPhysicalMultiFactorAuth = "http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical";
        }

        // Standard scopes defined by OPEN-ID
        public static class StandardScopes
        {
            public static Scope ProfileScope = new Scope
            {
                Name = "profile",
                IsExposed = true,
                IsInternal = true,
                IsDisplayedInConsent = true,
                Description = "Access to the profile",
                Claims = new List<string>
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                    Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                    Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                    Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                    Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                    Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                    Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                    Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                    Jwt.Constants.StandardResourceOwnerClaimNames.WebSite,
                    Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                    Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                    Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                    Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                    Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt
                },
                Type = ScopeType.ResourceOwner
            };

            public static Scope Email = new Scope
            {
                Name = "email",
                IsExposed = true,
                IsInternal = true,
                IsDisplayedInConsent = true,
                Description = "Access to the email",
                Claims = new List<string>
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                    Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified
                },
                Type = ScopeType.ResourceOwner
            };

            public static Scope Address = new Scope
            {
                Name = "address",
                IsExposed = true,
                IsInternal = true,
                IsDisplayedInConsent = true,
                Description = "Access to the address",
                Claims = new List<string>
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.Address
                },
                Type = ScopeType.ResourceOwner
            };

            public static Scope Phone = new Scope
            {
                Name = "phone",
                IsExposed = true,
                IsInternal = true,
                IsDisplayedInConsent = true,
                Description = "Access to the phone",
                Claims = new List<string>
                {
                    Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                    Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
                },
                Type = ScopeType.ResourceOwner
            };

            public static Scope OpenId = new Scope
            {
                Name = "openid",
                IsExposed = true,
                IsInternal = true,
                IsDisplayedInConsent = false,
                Description = "openid",
                Type = ScopeType.ProtectedApi
            };
        }

        // Defines the Assurance Level
        // For more information check this documentation : http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf
        public enum StandardNistAssuranceLevel
        {
            Level1 = 1,
            Level2 = 2,
            Level3 = 3,
            Level4 = 4
        }

        public static class StandardTokenTypes
        {
            public static string Bearer = "Bearer";
        }

        #endregion

        #region Internal definitions

        // Custom authentication policies defined by Simple Identity Server
        public static class CustomAuthenticationPolicies
        {
            public static string CustomPasswordAuth = "http://schemas.simpleidentityserver.net/pape/policies/2015/05/password";
        }

        public static readonly Dictionary<List<ResponseType>, AuthorizationFlow> MappingResponseTypesToAuthorizationFlows = new Dictionary<List<ResponseType>, AuthorizationFlow>
        {
            {
                new List<ResponseType>
                {
                    ResponseType.code
                },
                AuthorizationFlow.AuthorizationCodeFlow
            },
            {
                new List<ResponseType>
                {
                    ResponseType.id_token
                }, 
                AuthorizationFlow.ImplicitFlow
            },
            {
                new List<ResponseType>
                {
                    ResponseType.id_token,
                    ResponseType.token
                }, 
                AuthorizationFlow.ImplicitFlow
            },
            {
                new List<ResponseType>
                {
                    ResponseType.code,
                    ResponseType.id_token
                }, 
                AuthorizationFlow.HybridFlow
            },
            {
                new List<ResponseType>
                {
                    ResponseType.code,
                    ResponseType.token
                }, 
                AuthorizationFlow.HybridFlow
            },
            {
                new List<ResponseType>
                {
                    ResponseType.code,
                    ResponseType.id_token,
                    ResponseType.token
                }, 
                AuthorizationFlow.ImplicitFlow
            }
        };

        public static class Supported
        {
            public static List<AuthorizationFlow> SupportedAuthorizationFlows = new List<AuthorizationFlow>
            {
                AuthorizationFlow.AuthorizationCodeFlow,
                AuthorizationFlow.ImplicitFlow
            };

            public static List<GrantType> SupportedGrantTypes = new List<GrantType>
            {
                GrantType.@implicit,
                GrantType.authorization_code
            }; 

            public static List<string> SupportedResponseModes = new List<string>
            {
                "query"
            }; 

            public static List<string> SupportedSubjectTypes = new List<string>
            {
                // Same subject value to all clients.
                "public"
            };

            public static List<TokenEndPointAuthenticationMethods> SupportedTokenEndPointAuthenticationMethods = new List
                <TokenEndPointAuthenticationMethods>
            {
                TokenEndPointAuthenticationMethods.client_secret_basic,
                TokenEndPointAuthenticationMethods.client_secret_post,
                TokenEndPointAuthenticationMethods.client_secret_jwt
            };
        }

        #endregion
    }
}