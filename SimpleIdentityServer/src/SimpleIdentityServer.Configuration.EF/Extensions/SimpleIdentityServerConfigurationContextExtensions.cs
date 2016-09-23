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
                    // OAUTH20
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Type = 1,
                        Name = "Facebook",
                        CallbackPath = "/signin-facebook",
                        Code = "using Newtonsoft.Json.Linq;\r\nusing SimpleIdentityServer.Core.Jwt;\r\nusing System.Collections.Generic;\r\nusing System.Security.Claims;\r\n\r\nnamespace Parser\r\n{\r\n    public class FacebookClaimsParser\r\n    {\r\n        private readonly Dictionary<string, string> _mappingFacebookClaimToOpenId = new Dictionary<string, string>\r\n        {\r\n            {\r\n                \"id\",\r\n                Constants.StandardResourceOwnerClaimNames.Subject\r\n            },\r\n            {\r\n                \"name\",\r\n                Constants.StandardResourceOwnerClaimNames.Name\r\n            },\r\n            {\r\n                \"first_name\",\r\n                Constants.StandardResourceOwnerClaimNames.GivenName\r\n            },\r\n            {\r\n                \"last_name\",\r\n                Constants.StandardResourceOwnerClaimNames.FamilyName\r\n            },\r\n            {\r\n                \"gender\",\r\n                Constants.StandardResourceOwnerClaimNames.Gender\r\n            },\r\n            {\r\n                \"locale\",\r\n                Constants.StandardResourceOwnerClaimNames.Locale\r\n            },\r\n            {\r\n                \"picture\",\r\n                Constants.StandardResourceOwnerClaimNames.Picture\r\n            },\r\n            {\r\n                \"updated_at\",\r\n                Constants.StandardResourceOwnerClaimNames.UpdatedAt\r\n            },\r\n            {\r\n                \"email\",\r\n                Constants.StandardResourceOwnerClaimNames.Email\r\n            },\r\n            {\r\n                \"birthday\",\r\n                Constants.StandardResourceOwnerClaimNames.BirthDate\r\n            },\r\n            {\r\n                \"link\",\r\n                Constants.StandardResourceOwnerClaimNames.WebSite\r\n            }\r\n        };\r\n\r\n        public List<Claim> Process(JObject jObj)\r\n        {\r\n            var result = new List<Claim>();\r\n            foreach(var claim in jObj)\r\n            {\r\n                string key = claim.Key;\r\n                if (_mappingFacebookClaimToOpenId.ContainsKey(claim.Key))\r\n                {\r\n                    key = _mappingFacebookClaimToOpenId[claim.Key];\r\n                }\r\n\r\n                result.Add(new Claim(key, claim.Value.ToString()));\r\n            }\r\n\r\n            return result;\r\n        }\r\n    }\r\n}",
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
                        Type = 1,
                        Name = "Google",
                        CallbackPath = "/signin-google",
                        Code = "using Newtonsoft.Json.Linq;\r\nusing SimpleIdentityServer.Core.Jwt;\r\nusing System.Collections.Generic;\r\nusing System.Security.Claims;\r\n\r\nnamespace Parser\r\n{\r\n    public class GoogleClaimsParser\r\n    {\r\n        private readonly Dictionary<string, string> _mappingFacebookClaimToOpenId = new Dictionary<string, string>\r\n        {\r\n            {\r\n                \"id\",\r\n                Constants.StandardResourceOwnerClaimNames.Subject\r\n            },\r\n            {\r\n                \"displayName\",\r\n                Constants.StandardResourceOwnerClaimNames.Name\r\n            },\r\n            {\r\n                \"name.givenName\",\r\n                Constants.StandardResourceOwnerClaimNames.GivenName\r\n            },\r\n            {\r\n                \"name.familyName\",\r\n                Constants.StandardResourceOwnerClaimNames.FamilyName\r\n            },\r\n            {\r\n                \"url\",\r\n                Constants.StandardResourceOwnerClaimNames.WebSite\r\n            },\r\n            {\r\n                \"emails.value\",\r\n                Constants.StandardResourceOwnerClaimNames.Email\r\n            }\r\n        };\r\n\r\n        public List<Claim> Process(JObject jObj)\r\n        {\r\n            var result = new List<Claim>();\r\n            foreach (var claim in jObj)\r\n            {\r\n                string key = claim.Key;\r\n                foreach(var mapping in _mappingFacebookClaimToOpenId)\r\n                {\r\n                    var val = GetValue(claim, mapping);\r\n                    if (val != null)\r\n                    {\r\n                        result.Add(new Claim(mapping.Value, val));\r\n                        break;\r\n                    }\r\n                }\r\n            }\r\n\r\n            return result;\r\n        }\r\n\r\n        private string GetValue(\r\n            KeyValuePair<string, JToken> claim,\r\n            KeyValuePair<string, string> mapping,\r\n            int indice = 0)\r\n        {\r\n            var splitted = mapping.Key.Split('.');\r\n            if (splitted.Length < 1 ||\r\n                splitted.Length <= indice ||\r\n                splitted[indice] != claim.Key)\r\n            {\r\n                return null;\r\n            }\r\n\r\n            var dic = claim.Value as JObject;\r\n            if (dic == null)\r\n            {\r\n                return claim.Value.ToString();\r\n            }\r\n\r\n            indice = indice + 1;\r\n            foreach (var record in dic)\r\n            {\r\n                var val = GetValue(record, mapping, indice);\r\n                if (val != null)\r\n                {\r\n                    return val;\r\n                }\r\n            }\r\n\r\n            return null;         \r\n        }\r\n    }\r\n}\r\n",
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
                        Code = "using Newtonsoft.Json.Linq;\r\nusing SimpleIdentityServer.Core.Jwt;\r\nusing System.Collections.Generic;\r\nusing System.Security.Claims;\r\n\r\nnamespace Parser\r\n{\r\n    public class GitHubClaimsParser\r\n    {\r\n        private readonly Dictionary<string, string> _mappingFacebookClaimToOpenId = new Dictionary<string, string>\r\n        {\r\n            {\r\n                \"id\",\r\n                Constants.StandardResourceOwnerClaimNames.Subject\r\n            },\r\n            {\r\n                \"name\",\r\n                Constants.StandardResourceOwnerClaimNames.Name\r\n            },\r\n            {\r\n                \"avatar_url\",\r\n                Constants.StandardResourceOwnerClaimNames.Picture\r\n            },\r\n            {\r\n                \"updated_at\",\r\n                Constants.StandardResourceOwnerClaimNames.UpdatedAt\r\n            },\r\n            {\r\n                \"email\",\r\n                Constants.StandardResourceOwnerClaimNames.Email\r\n            }\r\n        };\r\n\r\n        public List<Claim> Process(JObject jObj)\r\n        {\r\n            var result = new List<Claim>();\r\n            foreach (var claim in jObj)\r\n            {\r\n                string key = claim.Key;\r\n                if (_mappingFacebookClaimToOpenId.ContainsKey(claim.Key))\r\n                {\r\n                    key = _mappingFacebookClaimToOpenId[claim.Key];\r\n                }\r\n\r\n                result.Add(new Claim(key, claim.Value.ToString()));\r\n            }\r\n\r\n            return result;\r\n        }\r\n    }\r\n}\r\n",
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
                        Code = "using Newtonsoft.Json.Linq;\r\nusing SimpleIdentityServer.Core.Jwt;\r\nusing System.Collections.Generic;\r\nusing System.Security.Claims;\r\n\r\nnamespace Parser\r\n{\r\n    public class LinkedinClaimsParser\r\n    {\r\n        private readonly Dictionary<string, string> _mappingLinkedinClaimsToOpenId = new Dictionary<string, string>\r\n        {\r\n            {\r\n                \"id\",\r\n                Constants.StandardResourceOwnerClaimNames.Subject\r\n            },\r\n            {\r\n                \"firstName\",\r\n                Constants.StandardResourceOwnerClaimNames.GivenName\r\n            },\r\n            {\r\n                \"lastName\",\r\n                Constants.StandardResourceOwnerClaimNames.FamilyName\r\n            }\r\n        };\r\n\r\n        public List<Claim> Process(JObject claims)\r\n        {\r\n            var result = new List<Claim>();\r\n            foreach (var claim in claims)\r\n            {\r\n                string key = claim.Key;\r\n                if (_mappingLinkedinClaimsToOpenId.ContainsKey(claim.Key))\r\n                {\r\n                    key = _mappingLinkedinClaimsToOpenId[claim.Key];\r\n                }\r\n\r\n                result.Add(new Claim(key, claim.Value.ToString()));\r\n            }\r\n\r\n            return result;\r\n        }\r\n    }\r\n}\r\n",
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
                    },
                    // OPENID                    
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Type = 2,
                        Name = "Microsoft",
                        CallbackPath = "/signin-microsoft",
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
                    // WSFEDERATION
                    new AuthenticationProvider
                    {
                        IsEnabled = true,
                        Type = 3,
                        Name = "Eid",
                        CallbackPath = "/signin-eid",
                        Code = "using SimpleIdentityServer.Core.Jwt;\r\nusing System.Collections.Generic;\r\nusing System.Security.Claims;\r\nusing System.Xml;\r\n\r\nnamespace Parser\r\n{\r\n    public class EidClaimsParser\r\n    {\r\n        private static Dictionary<string, string> _mappingWifToOpenIdClaims = new Dictionary<string, string>\r\n        {\r\n            {\r\n                ClaimTypes.Gender, Constants.StandardResourceOwnerClaimNames.Gender\r\n            },\r\n            {\r\n                ClaimTypes.GivenName, Constants.StandardResourceOwnerClaimNames.GivenName\r\n            },\r\n            {\r\n                ClaimTypes.Name, Constants.StandardResourceOwnerClaimNames.Name\r\n            },\r\n            {\r\n                ClaimTypes.Role,Constants.StandardResourceOwnerClaimNames.Role\r\n            },\r\n            {\r\n                ClaimTypes.DateOfBirth, Constants.StandardResourceOwnerClaimNames.BirthDate\r\n            },\r\n            {\r\n                ClaimTypes.Email, Constants.StandardResourceOwnerClaimNames.Email\r\n            },\r\n            {\r\n                ClaimTypes.MobilePhone, Constants.StandardResourceOwnerClaimNames.PhoneNumber\r\n            },\r\n            {\r\n                ClaimTypes.HomePhone, Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified\r\n            },\r\n            {\r\n                ClaimTypes.Webpage, Constants.StandardResourceOwnerClaimNames.WebSite\r\n            }\r\n        };\r\n\r\n        public List<Claim> Process(XmlNode node)\r\n        {\r\n            var result = new List<Claim>();\r\n            foreach(XmlNode child in node.ChildNodes)\r\n            {\r\n                if (child.Name == \"saml2:Subject\" || child.Name == \"saml:Subject\")\r\n                {\r\n                    result.Add(new Claim(Constants.StandardResourceOwnerClaimNames.Subject, child.InnerText));\r\n                }\r\n\r\n                if (child.Name == \"saml2:AttributeStatement\"\r\n                    || child.Name == \"saml:AttributeStatement\")\r\n                {\r\n                    foreach (XmlNode attribute in child.ChildNodes)\r\n                    {\r\n                        var id = string.Empty;\r\n                        foreach (XmlAttribute metadata in attribute.Attributes)\r\n                        {\r\n                            if (metadata.Name == \"Name\")\r\n                            {\r\n                                id = metadata.Value;\r\n                                break;\r\n                            }\r\n                        }\r\n\r\n                        if (_mappingWifToOpenIdClaims.ContainsKey(id))\r\n                        {\r\n                            id = _mappingWifToOpenIdClaims[id];\r\n                        }\r\n\r\n                        result.Add(new Claim(id, attribute.InnerText));\r\n                    }\r\n                }\r\n            }\r\n\r\n            return result;\r\n        }\r\n    }\r\n}\r\n",
                        ClassName = "EidClaimsParser",
                        Namespace = "Parser",
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
                    }
                });
            }
        }

        private static void InsertSettings(SimpleIdentityServerConfigurationContext context)
        {
            if (!context.Settings.Any())
            {
                context.Settings.AddRange(new[] 
                {
                    // Expiration times
                     new Setting
                    {
                        Key = Constants.SettingNames.ExpirationTimeName,
                        Value = "3600"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.AuthorizationCodeExpirationTimeName,
                        Value = "3600"
                    },
                    // Email settings
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailFromName,
                        Value = "Lokit"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailFromAddress,
                        Value = "lokitserver@hotmail.com"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailSubject,
                        Value = "Confirmation code"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailBody,
                        Value = "Your confirmation code is {0}"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailSmtpHost,
                        Value = "smtp.live.com"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailSmtpPort,
                        Value = "587"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailSmtpUseSsl,
                        Value = "false"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailUserName,
                        Value = "lokitserver@hotmail.com"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.EmailPassword,
                        Value = "FuckMicrosoftPassword1989"
                    },
                    // Twilio
                    new Setting
                    {
                        Key = Constants.SettingNames.TwilioAccountSid,
                        Value = "AC093c9783bfa2e70ff29998c2b3d1ba5a"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.TwilioAuthToken,
                        Value = "0c006b20fa2459200274229b2b655746"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.TwilioFromNumber,
                        Value = "+32460206628"
                    },
                    new Setting
                    {
                        Key = Constants.SettingNames.TwilioMessage,
                        Value = "Your code is {0}"
                    }
                });
            }
        }

        #endregion
    }
}