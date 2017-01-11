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

using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Test
{
    class Program
    {
        private const string _baseUrl = "https://rp.certification.openid.net:8080/simpleIdServer";
        private const string RedirectUriCode = "https://localhost:5106/Authenticate/Callback";
        private static IJwsParser _jwsParser;
        private static IJsonWebKeyConverter _jsonWebKeyConverter;

        public static void Main(string[] args)
        {
            _jwsParser = new JwsParser(null);
            _jsonWebKeyConverter = new JsonWebKeyConverter();
            // 1. Execute tests for basic profile
            RpResponseTypeCode().Wait();
            RpScopeUserInfoClaims().Wait();
            RpNonceInvalid().Wait();
            RpTokenEndpointClientSecretBasic().Wait();
            RpIdTokenAud().Wait();
            RpIdTokenKidAbsentSingleJwks().Wait();
            RpIdTokenSigNone().Wait();
            RpIdTokenIssuerMismatch().Wait();
            RpIdTokenKidAbsentMultipleJwks().Wait();
            RpIdTokenBadSigRS256().Wait();
            RpIdTokenIat().Wait();
            RpIdTokenSigRs256().Wait();
            RpIdTokenSub().Wait();
            // identityServerClientFactory.CreateAuthSelector()
            Console.ReadLine();
        }

        private static async Task RpResponseTypeCode()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-response_type-code.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-response_type-code/.well-known/openid-configuration");
                // rp-response_type-code : Make an authentication request using the "Authorization code Flow"
                Log("Register client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                        RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                        "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                        "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid",
                            Nonce = nonce
                        });
                Log($"Authorization code has been returned {result.Content.Value<string>("code")}", writer);
            }
        }

        private static async Task RpScopeUserInfoClaims()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-scope-userinfo-claims.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-scope-userinfo-claims/.well-known/openid-configuration");
                // rp-scope-userinfo-claims : Request claims using scope values.
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                        RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                        "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                        "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get authorization", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get access token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                Log("Get user information", writer);
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, token.AccessToken);
                Log($"claims has been returned, the subject is : {userInfo.Value<string>("sub")}", writer);
            }
        }

        private static async Task RpNonceInvalid()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-nonce-invalid.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-nonce-invalid/.well-known/openid-configuration");
                // rp-scope-userinfo-claims : Request claims using scope values.
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                        RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                        "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                        "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Nonce == nonce)
                {
                    Log("The nonce in identity token is correct", writer);
                }
                else
                {
                    Log("The nonce in identity token is not correct", writer);
                }
            }
        }

        private static async Task RpTokenEndpointClientSecretBasic()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-token_endpoint-client_secret_basic.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-token_endpoint-client_secret_basic/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        },
                        TokenEndpointAuthMethod = "client_secret_basic"
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get the identity token with client_secret_basic authentication method", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                Log($"Identity token returns {token.IdToken}", writer);
            }
        }

        private static async Task RpIdTokenAud()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-aud.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-aud/.well-known/openid-configuration");
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                        RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                        "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                        "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Audiences == null || !payload.Audiences.Any())
                {
                    Log("The audience is missing", writer);
                }
                else if (!payload.Audiences.Contains(client.ClientId))
                {
                    Log("The audience doesn't match the client id", writer);
                }
            }
        }

        private static async Task RpIdTokenKidAbsentSingleJwks()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-kid-absent-single-jwks.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-kid-absent-single-jwks/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var header = _jwsParser.GetHeader(token.IdToken);
                Log($"receive the identity token {token.IdToken}", writer);
                if (string.IsNullOrWhiteSpace(header.Kid))
                {
                    Log("the kid doesn't exist", writer);
                }
                else
                {
                    var jsonWebKeySet = await identityServerClientFactory.CreateJwksClient()
                        .ExecuteAsync(discovery.JwksUri);
                    var keys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    var key = keys.FirstOrDefault(k => k.Kid == header.Kid);
                    if (key == null)
                    {
                        Log("The kid doesn't exist", writer);
                    }
                    else
                    {
                        var jwsPayload = _jwsParser.ValidateSignature(token.IdToken, key);
                        if (jwsPayload == null)
                        {
                            Log("the json web key is not correct", writer);
                        }
                        else
                        {
                            Log($"identity token is correct {token.IdToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenSigNone()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-sig-none.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-sig-none/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        },
                        IdTokenSignedResponseAlg = "None"
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                Log($"the payload is valid {payload["sub"]}", writer);
            }
        }

        private static async Task RpIdTokenIssuerMismatch()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-issuer-mismatch.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-issuer-mismatch/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Issuer != _baseUrl + "/rp-id_token-issuer-mismatch")
                {
                    Log($"the issuer is not correct {payload.Issuer} != {_baseUrl + "/rp-id_token-issuer-mismatch"}", writer);
                }
                else
                {
                    Log("the issuer is correct", writer);
                }
            }
        }

        private static async Task RpIdTokenKidAbsentMultipleJwks()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-kid-absent-multiple-jwks.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-kid-absent-multiple-jwks/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var header = _jwsParser.GetHeader(token.IdToken);
                Log($"receive the identity token {token.IdToken}", writer);
                if (string.IsNullOrWhiteSpace(header.Kid))
                {
                    Log("the kid doesn't exist", writer);
                }
                else
                {
                    var jsonWebKeySet = await identityServerClientFactory.CreateJwksClient()
                        .ExecuteAsync(discovery.JwksUri);
                    var keys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    var key = keys.FirstOrDefault(k => k.Kid == header.Kid);
                    if (key == null)
                    {
                        Log("The kid doesn't exist", writer);
                    }
                    else
                    {
                        var jwsPayload = _jwsParser.ValidateSignature(token.IdToken, key);
                        if (jwsPayload == null)
                        {
                            Log("the json web key is not correct", writer);
                        }
                        else
                        {
                            Log($"identity token is correct {token.IdToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenBadSigRS256()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-bad-sig-rs256.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-bad-sig-rs256/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        },
                        IdTokenSignedResponseAlg = "RS256"
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var header = _jwsParser.GetHeader(token.IdToken);
                Log($"receive the identity token {token.IdToken}", writer);
                if (string.IsNullOrWhiteSpace(header.Kid))
                {
                    Log("the kid doesn't exist", writer);
                }
                else
                {
                    var jsonWebKeySet = await identityServerClientFactory.CreateJwksClient()
                        .ExecuteAsync(discovery.JwksUri);
                    var keys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    var key = keys.FirstOrDefault(k => k.Kid == header.Kid);
                    if (key == null)
                    {
                        Log("The kid doesn't exist", writer);
                    }
                    else
                    {
                        var jwsPayload = _jwsParser.ValidateSignature(token.IdToken, key);
                        if (jwsPayload == null)
                        {
                            Log("the id_token signature is not correct", writer);
                        }
                        else
                        {
                            Log($"identity token is correct {token.IdToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenIat()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-iat.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-iat/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Keys.Contains("iat"))
                {
                    Log($"the payload contains an iat {payload.Iat}", writer);
                }
                else
                {
                    Log("the payload doesn't contain iat", writer);
                }
            }
        }

        public static async Task RpIdTokenSigRs256()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-sig-rs256.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-sig-rs256/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        },
                        IdTokenSignedResponseAlg = "RS256"
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var header = _jwsParser.GetHeader(token.IdToken);
                Log($"receive the identity token {token.IdToken}", writer);
                if (string.IsNullOrWhiteSpace(header.Kid))
                {
                    Log("the kid doesn't exist", writer);
                }
                else
                {
                    var jsonWebKeySet = await identityServerClientFactory.CreateJwksClient()
                        .ExecuteAsync(discovery.JwksUri);
                    var keys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    var key = keys.FirstOrDefault(k => k.Kid == header.Kid);
                    if (key == null)
                    {
                        Log("The kid doesn't exist", writer);
                    }
                    else
                    {
                        var jwsPayload = _jwsParser.ValidateSignature(token.IdToken, key);
                        if (jwsPayload == null)
                        {
                            Log("the id_token signature is not correct", writer);
                        }
                        else
                        {
                            Log($"identity token is correct {token.IdToken}", writer);
                        }
                    }
                }
            }
        }

        public static async Task RpIdTokenSub()
        {
            using (var writer = File.AppendText(@"C:\Users\thabart\Desktop\Logs\rp-id_token-sub.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(_baseUrl + "/rp-id_token-sub/.well-known/openid-configuration");
                Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            RedirectUriCode
                        },
                        ApplicationType = "web",
                        GrantTypes = new List<string>
                        {
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "code"
                        }
                    }, discovery.RegistrationEndPoint);
                Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Keys.Contains("sub"))
                {
                    Log($"the payload contains a sub {payload["sub"]}", writer);
                }
                else
                {
                    Log("the payload doesn't contain sub", writer);
                }
            }
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.WriteLine($"Log Entry : {DateTime.UtcNow} : {logMessage}");
        }
    }
}
