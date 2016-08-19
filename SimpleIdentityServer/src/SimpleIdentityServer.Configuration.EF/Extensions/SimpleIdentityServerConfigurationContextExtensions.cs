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

using SimpleIdentityServer.Configuration.Core;
using SimpleIdentityServer.Configuration.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Configuration.EF.Extensions
{
    public static class SimpleIdentityServerConfigurationContextExtensions
    {
        #region Public static methods

        public static void EnsureSeedData(this SimpleIdentityServerConfigurationContext context)
        {
            InsertAuthenticationProviders(context);
            InsertSettings(context);
            context.SaveChanges();
        }

        #endregion

        #region Private static methods

        private static void InsertAuthenticationProviders(SimpleIdentityServerConfigurationContext context)
        {
            if (!context.AuthenticationProviders.Any())
            {
                context.AuthenticationProviders.AddRange(new[]
                {
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Type = 1,
                        Name = "Facebook",
                        CallbackPath = "/signin-facebook",
                        Code = "using SimpleIdentityServer.Core.Jwt;\r\nusing System.Collections.Generic;\r\n\r\nnamespace Parser\r\n{\r\n    public class FacebookClaimsParser\r\n    {\r\n        private readonly Dictionary<string, string> _mappingFacebookClaimToOpenId = new Dictionary<string, string>\r\n        {\r\n            {\r\n                \"id\",\r\n                Constants.StandardResourceOwnerClaimNames.Subject\r\n            },\r\n            {\r\n                \"name\",\r\n                Constants.StandardResourceOwnerClaimNames.Name\r\n            },\r\n            {\r\n                \"first_name\",\r\n                Constants.StandardResourceOwnerClaimNames.GivenName\r\n            },\r\n            {\r\n                \"last_name\",\r\n                Constants.StandardResourceOwnerClaimNames.FamilyName\r\n            },\r\n            {\r\n                \"gender\",\r\n                Constants.StandardResourceOwnerClaimNames.Gender\r\n            },\r\n            {\r\n                \"locale\",\r\n                Constants.StandardResourceOwnerClaimNames.Locale\r\n            },\r\n            {\r\n                \"picture\",\r\n                Constants.StandardResourceOwnerClaimNames.Picture\r\n            },\r\n            {\r\n                \"updated_at\",\r\n                Constants.StandardResourceOwnerClaimNames.UpdatedAt\r\n            },\r\n            {\r\n                \"email\",\r\n                Constants.StandardResourceOwnerClaimNames.Email\r\n            },\r\n            {\r\n                \"birthday\",\r\n                Constants.StandardResourceOwnerClaimNames.BirthDate\r\n            },\r\n            {\r\n                \"link\",\r\n                Constants.StandardResourceOwnerClaimNames.WebSite\r\n            }\r\n        };\r\n\r\n        public Dictionary<string, object> Process(Dictionary<string, object> claims)\r\n        {\r\n            var result = new Dictionary<string, object>();\r\n            foreach(var claim in claims)\r\n            {\r\n                string key = claim.Key;\r\n                if (_mappingFacebookClaimToOpenId.ContainsKey(claim.Key))\r\n                {\r\n                    key = _mappingFacebookClaimToOpenId[claim.Key];\r\n                }\r\n\r\n                result.Add(key, claim.Value);\r\n            }\r\n\r\n            return result;\r\n        }\r\n    }\r\n}",
                        ClassName = "FacebookClaimsParser",
                        Namespace = "Parser",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "569242033233529"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "12e0f33817634c0a650c0121d05e53eb"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AuthorizationEndpoint",
                                Value = "https://www.facebook.com/v2.6/dialog/oauth"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "TokenEndpoint",
                                Value = "https://graph.facebook.com/v2.6/oauth/access_token"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "UserInformationEndpoint",
                                Value = "https://graph.facebook.com/v2.6/me"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "email"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "public_profile"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Type = 2,
                        Name = "Microsoft",
                        CallbackPath = "/signin-microsoft",
                        Code = "code",
                        ClassName = "MicrosoftClaimsParser",
                        Namespace = "Parser",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "59b073ec-cd5e-4616-bf6d-7a78312fc4a8"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "8NHDwaWR9pqPzQQKchNOeza"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "WellKnownConfigurationEndPoint",
                                Value = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "openid"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "profile"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Type = 1,
                        Name = "Google",
                        CallbackPath = "/signin-google",
                        Code = "using SimpleIdentityServer.Core.Jwt;\r\nusing System.Collections.Generic;\r\n\r\nnamespace Parser\r\n{\r\n    public class GoogleClaimsParser\r\n    {\r\n        private readonly Dictionary<string, string> _mappingFacebookClaimToOpenId = new Dictionary<string, string>\r\n        {\r\n            {\r\n                \"id\",\r\n                Constants.StandardResourceOwnerClaimNames.Subject\r\n            },\r\n            {\r\n                \"displayName\",\r\n                Constants.StandardResourceOwnerClaimNames.Name\r\n            },\r\n            {\r\n                \"name.givenName\",\r\n                Constants.StandardResourceOwnerClaimNames.GivenName\r\n            },\r\n            {\r\n                \"name.familyName\",\r\n                Constants.StandardResourceOwnerClaimNames.FamilyName\r\n            },\r\n            {\r\n                \"url\",\r\n                Constants.StandardResourceOwnerClaimNames.WebSite\r\n            },\r\n            {\r\n                \"emails.value\",\r\n                Constants.StandardResourceOwnerClaimNames.Email\r\n            }\r\n        };\r\n\r\n        public Dictionary<string, object> Process(Dictionary<string, object> claims)\r\n        {\r\n            var result = new Dictionary<string, object>();\r\n            foreach (var claim in claims)\r\n            {\r\n                string key = claim.Key;\r\n                foreach(var mapping in _mappingFacebookClaimToOpenId)\r\n                {\r\n                    var val = GetValue(claim, mapping);\r\n                    if (val != null)\r\n                    {\r\n                        result.Add(mapping.Value, val);\r\n                        break;\r\n                    }\r\n                }\r\n            }\r\n\r\n            return result;\r\n        }\r\n\r\n        private string GetValue(\r\n            KeyValuePair<string, object> claim,\r\n            KeyValuePair<string, string> mapping,\r\n            int indice = 0)\r\n        {\r\n            var splitted = mapping.Key.Split('.');\r\n            if (splitted.Length < 1 ||\r\n                splitted.Length <= indice ||\r\n                splitted[indice] != claim.Key)\r\n            {\r\n                return null;\r\n            }\r\n\r\n            var dic = claim.Value as Dictionary<string, object>;\r\n            if (dic == null)\r\n            {\r\n                return claim.Value.ToString();\r\n            }\r\n\r\n            indice = indice + 1;\r\n            foreach (var record in dic)\r\n            {\r\n                var val = GetValue(record, mapping, indice);\r\n                if (val != null)\r\n                {\r\n                    return val;\r\n                }\r\n            }\r\n\r\n            return null;         \r\n        }\r\n    }\r\n}\r\n",
                        ClassName = "GoogleClaimsParser",
                        Namespace = "Parser",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "636126285787-755svbmi6j75t54e00lk58fi1t1qs4c6.apps.googleusercontent.com"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "l-3B1I0hGNc-0S4NSdkIw2yE"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AuthorizationEndpoint",
                                Value = "https://accounts.google.com/o/oauth2/auth"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "TokenEndpoint",
                                Value = "https://www.googleapis.com/oauth2/v4/token"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "UserInformationEndpoint",
                                Value = "https://www.googleapis.com/plus/v1/people/me"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "openid"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "profile"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "email"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Type = 1,
                        Name = "Github",
                        CallbackPath = "/signin-github",
                        Code = "using SimpleIdentityServer.Core.Jwt;\r\nusing System.Collections.Generic;\r\n\r\nnamespace Parser\r\n{\r\n    public class GitHubClaimsParser\r\n    {\r\n        private readonly Dictionary<string, string> _mappingFacebookClaimToOpenId = new Dictionary<string, string>\r\n        {\r\n            {\r\n                \"id\",\r\n                Constants.StandardResourceOwnerClaimNames.Subject\r\n            },\r\n            {\r\n                \"name\",\r\n                Constants.StandardResourceOwnerClaimNames.Name\r\n            },\r\n            {\r\n                \"avatar_url\",\r\n                Constants.StandardResourceOwnerClaimNames.Picture\r\n            },\r\n            {\r\n                \"updated_at\",\r\n                Constants.StandardResourceOwnerClaimNames.UpdatedAt\r\n            },\r\n            {\r\n                \"email\",\r\n                Constants.StandardResourceOwnerClaimNames.Email\r\n            }\r\n        };\r\n\r\n        public Dictionary<string, object> Process(Dictionary<string, object> claims)\r\n        {\r\n            var result = new Dictionary<string, object>();\r\n            foreach (var claim in claims)\r\n            {\r\n                string key = claim.Key;\r\n                if (_mappingFacebookClaimToOpenId.ContainsKey(claim.Key))\r\n                {\r\n                    key = _mappingFacebookClaimToOpenId[claim.Key];\r\n                }\r\n\r\n                result.Add(key, claim.Value);\r\n            }\r\n\r\n            return result;\r\n        }\r\n    }\r\n}\r\n",
                        ClassName = "GitHubClaimsParser",
                        Namespace = "Parser",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "e03f5eb28418ee141944"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "1ca11063515064b7c2638924280ca026f9713f5b"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AuthorizationEndpoint",
                                Value = "https://github.com/login/oauth/authorize"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "TokenEndpoint",
                                Value = "https://github.com/login/oauth/access_token"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "UserInformationEndpoint",
                                Value = "https://api.github.com/user"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Type = 1,
                        Name = "Linkedin",
                        CallbackPath = "/signin-linkedin",
                        Code = "code",
                        ClassName = "LinkedinClaimsParser",
                        Namespace = "Parser",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "77ral6nw4t8ivj"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "FSJMGgpGEKbirIGJ"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AuthorizationEndpoint",
                                Value = "https://www.linkedin.com/oauth/v2/authorization"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "TokenEndpoint",
                                Value = "https://www.linkedin.com/oauth/v2/accessToken"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "UserInformationEndpoint",
                                Value = "https://api.linkedin.com/v1/people/~?format=json"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Scope",
                                Value = "r_basicprofile"
                            }
                        }
                    }
                    /*
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Name = "Twitter",
                        Type = 1,
                        Code = "code",
                        CallbackPath = "/signin-twitter",
                        ClassName = "TwitterClaimsParser",
                        Namespace = "Parser",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "g8so3MHsMdklZ8NHau1VfOcXB"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "Flxp1fR3XLVj2gsVpwSigJRy80sBdNUPum3CZUkeyyKwbzvlJz"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AuthorizationEndpoint",
                                Value = "https://api.twitter.com/oauth/authorize"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "TokenEndpoint",
                                Value = "https://api.twitter.com/oauth2/token"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "UserInformationEndpoint",
                                Value = "https://api.twitter.com/1.1/account/verify_credentials.json"
                            }
                        }
                    },
                    new AuthenticationProvider
                    {
                        IsEnabled = false,
                        Name = "ADFS",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientId",
                                Value = "clientid"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClientSecret",
                                Value = "clientsecret"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "RelyingParty",
                                Value = "rp"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "ClaimsIssuer",
                                Value = "url://ci"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AdfsAuthorizationEndPoint",
                                Value = "https://adfs.mycompany.com/adfs/oauth2/authorize/"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "AdfsTokenEndPoint",
                                Value = "https://adfs.mycompany.com/adfs/oauth2/token/"
                            }
                        }
                    },
					new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Name = "EID",
                        Options = new List<Option>
                        {
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "IdPEndpoint",
                                Value = "https://www.e-contract.be/eid-idp/protocol/ws-federation/auth-ident"
                            },
                            new Option
                            {
                                Id = Guid.NewGuid().ToString(),
                                Key = "Realm",
                                Value = "urn://idserver"
                            }
                        }
                    },*/
                });
            }
        }

        private static void InsertSettings(SimpleIdentityServerConfigurationContext context)
        {
            if (!context.Settings.Any())
            {
                context.Settings.AddRange(new[] 
                {
                     new Setting
                    {
                        Key = Constants.SettingNames.ExpirationTimeName,
                        Value = "3600"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.AuthorizationCodeExpirationTimeName,
                        Value = "3600"
                    }
                });
            }
        }

        #endregion
    }
}
