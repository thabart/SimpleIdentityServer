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

using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.EF;
using SimpleIdentityServer.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

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
                        IsDisplayedInConsent = false,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new Scope
                    {
                        Name = "manager",
                        Description = "Access to the manager",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = false,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new Scope
                    {
                        Name = "scim_read",
                        Description = "Access to SCIM",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = false,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new Scope
                    {
                        Name = "scim_manage",
                        Description = "Manage SCIM resources",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = false,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
					new Scope
					{						
                        Name = "get_parameters",
                        Description = "Get the parameters",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = false,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
					},
					new Scope
					{						
                        Name = "add_parameters",
                        Description = "Add the parameters",
                        IsOpenIdScope = false,
                        IsDisplayedInConsent = false,
                        Type = ScopeType.ProtectedApi,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
					}
                });
            }
        }
                
        private static void InsertJsonWebKeys(SimpleIdentityServerContext context)
        {
            if (!context.JsonWebKeys.Any())
            {
                var serializedRsa = string.Empty;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    using (var provider = new RSACryptoServiceProvider())
                    {
                        serializedRsa = provider.ToXmlStringNetCore(true);
                    }
                }
                else
                {
                    using (var rsa = new RSAOpenSsl())
                    {
                        serializedRsa = rsa.ToXmlStringNetCore(true);
                    }
                }

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
                    new SimpleIdentityServer.EF.Models.Client // Configure the client needed to introspect the access_token.
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
                        ApplicationType = ApplicationTypes.native,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new SimpleIdentityServer.EF.Models.Client // Configure the client which needs to access to the token edp (grant_type = uma_ticket)
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
                        ResponseTypes = "1",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new SimpleIdentityServer.EF.Models.Client // Configure the client which need to access to the permission endpoint.
                    {
                        ClientId = "ResourceServer",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "LW46am54neU/[=Su"
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
                            },
                            new ClientScope
                            {
                                ScopeName = "manager"
                            },
                            new ClientScope
                            {
                                ScopeName = "scim_read"
                            },
                            new ClientScope
                            {
                                ScopeName = "scim_manage"
                            },
							new ClientScope
							{
								ScopeName = "add_parameters"
							},
							new ClientScope
							{
								ScopeName = "get_parameters"
							}
                        },
                        GrantTypes = "3",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new SimpleIdentityServer.EF.Models.Client // Configure the client which need to access to the permission endpoint.
                    {
                        ClientId = "DocumentManagementApi",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "QZhq68aE44BmYEX9"
                            }
                        },
                        ClientName = "DocumentManagementApi",
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
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new SimpleIdentityServer.EF.Models.Client // Configure the client which need to access to the permission endpoint.
                    {
                        ClientId = "ProtectedWebsite",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "ProtectedWebsite"
                            }
                        },
                        ClientName = "ProtectedWebsite",
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
                        GrantTypes = "3,5",
                        ResponseTypes = "1",
                        IdTokenSignedResponseAlg = "RS256",
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new SimpleIdentityServer.EF.Models.Client
                    {
                        ClientId = "OpenIdManager",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "Ex9'^s@?ADXX9nP_"
                            }
                        },
                        ClientName = "Manager",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post
                    },
                    new SimpleIdentityServer.EF.Models.Client
                    {
                        ClientId = "AuthManager",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "XD~}]7nYh5NS*N{U"
                            }
                        },
                        ClientName = "AuthManager",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post
                    },
                    new SimpleIdentityServer.EF.Models.Client
                    {
                        ClientId = "Scim",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "~V*nH{q4;qL/=8+Z"
                            }
                        },
                        ClientName = "SCIM",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new SimpleIdentityServer.EF.Models.Client
                    {
                        ClientId = "OpenId",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "z4Bp!:B@rFw4Xs+]"
                            }
                        },
                        ClientName = "OpenId",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    },
                    new SimpleIdentityServer.EF.Models.Client
                    {
                        ClientId = "EventStore",
                        ClientSecrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                Type = SecretTypes.SharedSecret,
                                Value = "9qj8XnSKYy8fPEkd"
                            }
                        },
                        ClientName = "EventStore",
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        UpdateDateTime = DateTime.UtcNow,
                        CreateDateTime = DateTime.UtcNow
                    }
                });
            }
        }
    }
}
