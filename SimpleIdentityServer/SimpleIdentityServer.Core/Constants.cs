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
using SimpleIdentityServer.Core.Api.Authorization;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;

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

        public static class StandardAuthorizationResponseNames
        {
            public static string IdTokenName = "id_token";

            public static string AccessTokenName = "access_token";

            public static string AuthorizationCodeName = "code";

            public static string StateName = "state";
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

        public static class StandardClaimParameterValueNames
        {
            public const string ValueName = "value";

            public const string ValuesName = "values";

            public const string EssentialName = "essential";
        }

        public static class StandardClaimParameterNames
        {
            public static string UserInfoName = "userinfo";

            public static string IdTokenName = "id_token";
        }

        public static List<string> AllStandardClaimParameterValueNames = new List<string>
        {
            StandardClaimParameterValueNames.ValueName,
            StandardClaimParameterValueNames.ValuesName,
            StandardClaimParameterValueNames.EssentialName
        };

        /// <summary>
        /// Parameter names of an authorization request
        /// </summary>
        public static class StandardAuthorizationRequestParameterNames
        {
            public static string ScopeName = "scope";
            public static string ResponseTypeName = "response_type";
            public static string ClientIdName = "client_id";
            public static string RedirectUriName = "redirect_uri";
            public static string StateName = "state";
            public static string ResponseModeName = "response_mode";
            public static string NonceName = "nonce";
            public static string DisplayName = "display";
            public static string PromptName = "prompt";
            public static string MaxAgeName = "max_age";
            public static string UiLocalesName = "ui_locales";
            public static string IdTokenHintName = "id_token_hint";
            public static string LoginHintName = "login_hint";
            public static string ClaimsName = "claims";
            public static string AcrValuesName = "acr_values";
            public static string RequestName = "request";
            public static string RequestUriName = "request_uri";
        }

        /// <summary>
        /// Read this RFC for more information
        /// </summary>
        public static class StandardRegistrationResponseNames
        {
            public const string ClientId = "client_id";
            public const string ClientSecret = "client_secret";
            public const string RegistrationAccessToken = "registration_access_token";
            public const string RegistrationClientUri = "registration_client_uri";
            public const string ClientIdIssuedAt = "client_id_issued_at";
            public const string ClientSecretExpiresAt = "client_secret_expires_at";
        }

        public static class StandardRegistrationRequestParameterNames
        {
            public const string RedirectUris = "redirect_uris";
            public const string ResponseTypes = "response_types";
            public const string GrantTypes = "grant_types";
            public const string ApplicationType = "application_type";
            public const string Contacts = "contacts";
            public const string ClientName = "client_name";
            public const string LogoUri = "logo_uri";
            public const string ClientUri = "client_uri";
            public const string PolicyUri = "policy_uri";
            public const string TosUri = "tos_uri";
            public const string JwksUri = "jwks_uri";
            public const string Jwks = "jwks";
            public const string SectoreIdentifierUri = "sector_identifier_uri";
            public const string SubjectType = "subject_type";
            public const string IdTokenSignedResponseAlg = "id_token_signed_response_alg";
            public const string IdTokenEncryptedResponseAlg = "id_token_encrypted_response_alg";
            public const string IdTokenEncryptedResponseEnc = "id_token_encrypted_response_enc";
            public const string UserInfoSignedResponseAlg = "userinfo_signed_response_alg";
            public const string UserInfoEncryptedResponseAlg = "userinfo_encrypted_response_alg";
            public const string UserInfoEncryptedResponseEnc = "userinfo_encrypted_response_enc";
            public const string RequestObjectSigningAlg = "request_object_signing_alg";
            public const string RequestObjectEncryptionAlg = "request_object_encryption_alg";
            public const string RequestObjectEncryptionEnc = "request_object_encryption_enc";
            public const string TokenEndPointAuthMethod = "token_endpoint_auth_method";
            public const string TokenEndPointAuthSigningAlg = "token_endpoint_auth_signing_alg";
            public const string DefaultMaxAge = "default_max_age";
            public const string RequireAuthTime = "require_auth_time";
            public const string DefaultAcrValues = "default_acr_values";
            public const string InitiateLoginUri = "initiate_login_uri";
            public const string RequestUris = "request_uris";
        }

        /// <summary>
        /// Parameter names of a token request
        /// </summary>
        public static class StandardTokenRequestParameterNames
        {
            public static string ClientIdName = "client_id";
            public static string UserName = "username";
            public static string PasswordName = "password";
            public static string AuthorizationCodeName = "code";
        }

        #endregion

        #region Internal definitions

        // Custom authentication policies defined by Simple Identity Server
        public static class CustomAuthenticationPolicies
        {
            public static string CustomPasswordAuth = "http://schemas.simpleidentityserver.net/pape/policies/2015/05/password";
        }

        public static class StandardTranslationCodes
        {
            public static string ApplicationWouldLikeToCode = "Application would like to";

            public static string ScopesCode = "Scopes";

            public static string IndividualClaimsCode = "individual claims";

            public static string LoginCode = "Login";

            public static string UserNameCode = "Username";

            public static string PasswordCode = "Password";

            public static string RememberMyLoginCode = "remember my login";

            public static string CancelCode = "cancel";

            public static string ConfirmCode = "confirm";
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
                AuthorizationFlow.HybridFlow
            }
        };

        public static Dictionary<AuthorizationFlow, ResponseMode> MappingAuthorizationFlowAndResponseModes = new Dictionary<AuthorizationFlow, ResponseMode>
        {
            {
                AuthorizationFlow.AuthorizationCodeFlow, ResponseMode.query
            },
            {
                AuthorizationFlow.ImplicitFlow, ResponseMode.fragment
            },
            {
                AuthorizationFlow.HybridFlow, ResponseMode.fragment
            }
        };

        public static class SubjectTypeNames
        {
            public const string Public = "public";
        }

        public static class Supported
        {
            public static List<AuthorizationFlow> SupportedAuthorizationFlows = new List<AuthorizationFlow>
            {
                AuthorizationFlow.AuthorizationCodeFlow,
                AuthorizationFlow.ImplicitFlow,
                AuthorizationFlow.HybridFlow
            };

            public static List<GrantType> SupportedGrantTypes = new List<GrantType>
            {
                GrantType.authorization_code,
                GrantType.client_credentials,
                GrantType.password,
                GrantType.refresh_token,
                GrantType.@implicit
            }; 

            public static List<string> SupportedResponseModes = new List<string>
            {
                "query"
            }; 

            public static List<string> SupportedSubjectTypes = new List<string>
            {
                // Same subject value to all clients.
                SubjectTypeNames.Public
            };

            public static List<string> SupportedJwsAlgs = new List<string>
            {
                Jwt.Constants.JwsAlgNames.RS256
            }; 

            public static List<string> SupportedJweAlgs = new List<string>
            {
                Jwt.Constants.JweAlgNames.RSA1_5
            }; 

            public static List<string> SupportedJweEncs = new List<string>
            {
                Jwt.Constants.JweEncNames.A128CBC_HS256
            }; 

            public static List<TokenEndPointAuthenticationMethods> SupportedTokenEndPointAuthenticationMethods = new List
                <TokenEndPointAuthenticationMethods>
            {
                TokenEndPointAuthenticationMethods.client_secret_basic,
                TokenEndPointAuthenticationMethods.client_secret_post
            };

            public static List<string> SupportedClaims = new List<string>
            {
                Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
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
                Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt,
                Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified,
                Jwt.Constants.StandardResourceOwnerClaimNames.Address,
                Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified
            };
        }

        #endregion
    }
}