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

using SimpleIdentityServer.DataAccess.SqlServer;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using SimpleIdentityServer.Uma.Startup.Extensions;

namespace SimpleIdentityServer.Uma.Startup.Extensions
{
    public static class SimpleIdentityServerContextExtensions
    {
        public static void EnsureSeedData(this SimpleIdentityServerContext context)
        {
            InsertScopes(context);
            InsertJsonWebKeys(context);
            InsertClients(context);
            context.SaveChanges();
        }

        private static void InsertScopes(SimpleIdentityServerContext context)
        {
            if (!context.Scopes.Any())
            {
                context.Scopes.AddRange(new[] {
                    new Scope
                    {
                        Name = "uma_protection",
                        Description = "Access to UMA permission, resource set",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = true,
                        Type = ScopeType.ProtectedApi
                    }
                });
            }
        }
                
        private static void InsertJsonWebKeys(SimpleIdentityServerContext context)
        {
            if (!context.JsonWebKeys.Any())
            {
                var serializedRsa = string.Empty;
#if NET461
                using (var provider = new RSACryptoServiceProvider())
                {
                    serializedRsa = provider.ToXmlString(true);
                }
#else
                using (var rsa = new RSAOpenSsl())
                {
                    serializedRsa = rsa.ToXmlString(true);
                }
#endif

                context.JsonWebKeys.AddRange(new[]
                {
                    new JsonWebKey
                    {
                        Alg = AllAlg.RS256,
                        KeyOps = "0,1",
                        Kid = "1",
                        Kty = KeyType.RSA,
                        Use = Use.Sig,
                        SerializedKey = serializedRsa,
                    },
                    new JsonWebKey
                    {
                        Alg = AllAlg.RSA1_5,
                        KeyOps = "2,3",
                        Kid = "2",
                        Kty = KeyType.RSA,
                        Use = Use.Enc,
                        SerializedKey = serializedRsa,
                    }
                });
            }
        }

        private static void InsertClients(SimpleIdentityServerContext context)
        {
            if (!context.Clients.Any())
            {
                context.Clients.AddRange(new[]
                {
                    new SimpleIdentityServer.DataAccess.SqlServer.Models.Client // Configure the client needed to introspect the access_token.
                    {
                        ClientId = "uma",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "uma"
                            }
                        },
                        ClientName = "UMA API",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.native
                    },
                    new SimpleIdentityServer.DataAccess.SqlServer.Models.Client // Configure the client which need to access to the permission endpoint.
                    {
                        ClientId = "resource_server",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "resource_server"
                            }
                        },
                        ClientName = "Resource server",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.native,
                        ClientScopes = new List<ClientScope>
                        {
                            new ClientScope
                            {
                                ScopeName=  "uma_protection"
                            }
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                    },
                    new SimpleIdentityServer.DataAccess.SqlServer.Models.Client // Configure the client which needs to access to the token edp (grant_type = uma_ticket)
                    {
                        ClientId = "client",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "client"
                            }
                        },
                        ClientName = "Client",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        PolicyUri = "http://openid.net",
                        TosUri = "http://openid.net",
                        ApplicationType = ApplicationTypes.native,
                        GrantTypes = "5",
                        ResponseTypes = "1"
                    }
                });
            }
        }
    }
}
