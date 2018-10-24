using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Uma.Host.Tests.Stores
{
    public static class OAuthStores
    {
        public static List<Scope> GetScopes()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "uma_protection",
                    Description = "Access to UMA permission, resource set & token introspection endpoints",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Type = ScopeType.ProtectedApi
                },
                new Scope
                {
                    Name = "uma_authorization",
                    Description = "Access to the UMA authorization endpoint",
                    IsOpenIdScope = false,
                    IsDisplayedInConsent = true,
                    Type = ScopeType.ProtectedApi
                }
            };
        }

        public static List<JsonWebKey> GetJsonWebKeys(SharedContext sharedContext)
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
            return new List<JsonWebKey>
            {
                sharedContext.EncryptionKey,
                sharedContext.SignatureKey
            };
        }

        public static List<SimpleIdentityServer.Core.Common.Models.Client> GetClients()
        {
            return new List<SimpleIdentityServer.Core.Common.Models.Client>
            {
                    // Resource server.
                    new SimpleIdentityServer.Core.Common.Models.Client
                    {
                        ClientId = "resource_server",
                        ClientName = "Resource server",
                        Secrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Type = ClientSecretTypes.SharedSecret,
                                Value = "resource_server"
                            }
                        },
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        AllowedScopes = new List<Scope>
                        {
                            new Scope
                            {
                                Name = "uma_protection"
                            },
                            new Scope
                            {
                                Name = "uma_authorization"
                            }
                        },
                        GrantTypes = new List<GrantType>
                        {
                            GrantType.client_credentials,
                            GrantType.uma_ticket
                        },
                        ResponseTypes = new List<ResponseType>
                        {
                            ResponseType.token
                        },
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.native
                    },
                    // Anonymous.
                    new SimpleIdentityServer.Core.Common.Models.Client
                    {
                        ClientId = "anonymous",
                        ClientName = "Anonymous",
                        Secrets = new List<ClientSecret>
                        {
                            new ClientSecret
                            {
                                Type = ClientSecretTypes.SharedSecret,
                                Value = "anonymous"
                            }
                        },
                        TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_post,
                        LogoUri = "http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg",
                        AllowedScopes = new List<Scope> {},
                        GrantTypes = new List<GrantType>
                        {
                            GrantType.client_credentials
                        },
                        ResponseTypes = new List<ResponseType>
                        {
                            ResponseType.token
                        },
                        IdTokenSignedResponseAlg = "RS256",
                        ApplicationType = ApplicationTypes.native
                    }

            };
        }

        private static string ComputeHash(string entry)
        {
            using (var sha256 = SHA256.Create())
            {
                var entryBytes = Encoding.UTF8.GetBytes(entry);
                var hash = sha256.ComputeHash(entryBytes);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
