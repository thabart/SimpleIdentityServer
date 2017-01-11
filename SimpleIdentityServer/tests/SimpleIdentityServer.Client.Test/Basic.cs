using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Serializer;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Test
{
    public static class Basic
    {
        private static IJwsParser _jwsParser;
        private static IJsonWebKeyConverter _jsonWebKeyConverter;
        private static string LogPath = @"C:\Users\thabart\Desktop\Logs\Basic\";

        public static async Task Start()
        {
            _jwsParser = new JwsParser(new CreateJwsSignature(new CngKeySerializer()));
            _jsonWebKeyConverter = new JsonWebKeyConverter();
            await RpResponseTypeCode();
            await RpScopeUserInfoClaims();
            await RpNonceInvalid();
            await RpTokenEndpointClientSecretBasic();
            await RpIdTokenAud();
            await RpIdTokenKidAbsentSingleJwks();
            await RpIdTokenSigNone();
            await RpIdTokenIssuerMismatch();
            await RpIdTokenKidAbsentMultipleJwks();
            await RpIdTokenBadSigRS256();
            await RpIdTokenIat();
            await RpIdTokenSigRs256();
            await RpIdTokenSub();
            await RpUserInfoBadSubClaim();
            await RpUserInfoBearerBody();
            await RpUserInfoBearerHeader();
        }
        
        private static async Task RpResponseTypeCode()
        {
            using (var writer = File.AppendText(LogPath + "rp-response_type-code.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-response_type-code/.well-known/openid-configuration");
                // rp-response_type-code : Make an authentication request using the "Authorization code Flow"
                Logger.Log("Register client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid",
                            Nonce = nonce
                        });
                Logger.Log($"Authorization code has been returned {result.Content.Value<string>("code")}", writer);
            }
        }

        private static async Task RpScopeUserInfoClaims()
        {
            using (var writer = File.AppendText(LogPath + "rp-scope-userinfo-claims.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-scope-userinfo-claims/.well-known/openid-configuration");
                // rp-scope-userinfo-claims : Request claims using scope values.
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                        Constants.RedirectUriCode
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
                Logger.Log("Get authorization", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get access token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                Logger.Log("Get user information", writer);
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, token.AccessToken);
                Logger.Log($"claims has been returned, the subject is : {userInfo.Value<string>("sub")}", writer);
            }
        }

        private static async Task RpNonceInvalid()
        {
            using (var writer = File.AppendText(LogPath + "rp-nonce-invalid.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-nonce-invalid/.well-known/openid-configuration");
                // rp-scope-userinfo-claims : Request claims using scope values.
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                        Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Nonce == nonce)
                {
                    Logger.Log("The nonce in identity token is correct", writer);
                }
                else
                {
                    Logger.Log("The nonce in identity token is not correct", writer);
                }
            }
        }

        private static async Task RpTokenEndpointClientSecretBasic()
        {
            using (var writer = File.AppendText(LogPath + "rp-token_endpoint-client_secret_basic.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-token_endpoint-client_secret_basic/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get the identity token with client_secret_basic authentication method", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                Logger.Log($"Identity token returns {token.IdToken}", writer);
            }
        }

        private static async Task RpIdTokenAud()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-aud.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-aud/.well-known/openid-configuration");
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                        Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Audiences == null || !payload.Audiences.Any())
                {
                    Logger.Log("The audience is missing", writer);
                }
                else if (!payload.Audiences.Contains(client.ClientId))
                {
                    Logger.Log("The audience doesn't match the client id", writer);
                }
            }
        }

        private static async Task RpIdTokenKidAbsentSingleJwks()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-kid-absent-single-jwks.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-kid-absent-single-jwks/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var header = _jwsParser.GetHeader(token.IdToken);
                Logger.Log($"receive the identity token {token.IdToken}", writer);
                if (string.IsNullOrWhiteSpace(header.Kid))
                {
                    Logger.Log("the kid doesn't exist", writer);
                }
                else
                {
                    var jsonWebKeySet = await identityServerClientFactory.CreateJwksClient()
                        .ExecuteAsync(discovery.JwksUri);
                    var keys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    var key = keys.FirstOrDefault(k => k.Kid == header.Kid);
                    if (key == null)
                    {
                        Logger.Log("The kid doesn't exist", writer);
                    }
                    else
                    {
                        var jwsPayload = _jwsParser.ValidateSignature(token.IdToken, key);
                        if (jwsPayload == null)
                        {
                            Logger.Log("the json web key is not correct", writer);
                        }
                        else
                        {
                            Logger.Log($"identity token is correct {token.IdToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenSigNone()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-sig-none.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-sig-none/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                        IdTokenSignedResponseAlg = "none"
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                Logger.Log($"the payload is valid {payload["sub"]}", writer);
            }
        }

        private static async Task RpIdTokenIssuerMismatch()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-issuer-mismatch.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-issuer-mismatch/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Issuer != Constants.BaseUrl + "/rp-id_token-issuer-mismatch")
                {
                    Logger.Log($"the issuer is not correct {payload.Issuer} != {Constants.BaseUrl + "/rp-id_token-issuer-mismatch"}", writer);
                }
                else
                {
                    Logger.Log("the issuer is correct", writer);
                }
            }
        }

        private static async Task RpIdTokenKidAbsentMultipleJwks()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-kid-absent-multiple-jwks.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-kid-absent-multiple-jwks/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var header = _jwsParser.GetHeader(token.IdToken);
                Logger.Log($"receive the identity token {token.IdToken}", writer);
                if (string.IsNullOrWhiteSpace(header.Kid))
                {
                    Logger.Log("the kid doesn't exist", writer);
                }
                else
                {
                    var jsonWebKeySet = await identityServerClientFactory.CreateJwksClient()
                        .ExecuteAsync(discovery.JwksUri);
                    var keys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    var key = keys.FirstOrDefault(k => k.Kid == header.Kid);
                    if (key == null)
                    {
                        Logger.Log("The kid doesn't exist", writer);
                    }
                    else
                    {
                        var jwsPayload = _jwsParser.ValidateSignature(token.IdToken, key);
                        if (jwsPayload == null)
                        {
                            Logger.Log("the json web key is not correct", writer);
                        }
                        else
                        {
                            Logger.Log($"identity token is correct {token.IdToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenBadSigRS256()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-bad-sig-rs256.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-bad-sig-rs256/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var header = _jwsParser.GetHeader(token.IdToken);
                Logger.Log($"receive the identity token {token.IdToken}", writer);
                if (string.IsNullOrWhiteSpace(header.Kid))
                {
                    Logger.Log("the kid doesn't exist", writer);
                }
                else
                {
                    var jsonWebKeySet = await identityServerClientFactory.CreateJwksClient()
                        .ExecuteAsync(discovery.JwksUri);
                    var keys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    var key = keys.FirstOrDefault(k => k.Kid == header.Kid);
                    if (key == null)
                    {
                        Logger.Log("The kid doesn't exist", writer);
                    }
                    else
                    {
                        var jwsPayload = _jwsParser.ValidateSignature(token.IdToken, key);
                        if (jwsPayload == null)
                        {
                            Logger.Log("the id_token signature is not correct", writer);
                        }
                        else
                        {
                            Logger.Log($"identity token is correct {token.IdToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenIat()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-iat.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-iat/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Keys.Contains("iat"))
                {
                    Logger.Log($"the payload contains an iat {payload.Iat}", writer);
                }
                else
                {
                    Logger.Log("the payload doesn't contain iat", writer);
                }
            }
        }

        private static async Task RpIdTokenSigRs256()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-sig-rs256.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-sig-rs256/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var header = _jwsParser.GetHeader(token.IdToken);
                Logger.Log($"receive the identity token {token.IdToken}", writer);
                if (string.IsNullOrWhiteSpace(header.Kid))
                {
                    Logger.Log("the kid doesn't exist", writer);
                }
                else
                {
                    var jsonWebKeySet = await identityServerClientFactory.CreateJwksClient()
                        .ExecuteAsync(discovery.JwksUri);
                    var keys = _jsonWebKeyConverter.ExtractSerializedKeys(jsonWebKeySet);
                    var key = keys.FirstOrDefault(k => k.Kid == header.Kid);
                    if (key == null)
                    {
                        Logger.Log("The kid doesn't exist", writer);
                    }
                    else
                    {
                        var jwsPayload = _jwsParser.ValidateSignature(token.IdToken, key);
                        if (jwsPayload == null)
                        {
                            Logger.Log("the id_token signature is not correct", writer);
                        }
                        else
                        {
                            Logger.Log($"identity token is correct {token.IdToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenSub()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-sub.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-sub/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an identity token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                if (payload.Keys.Contains("sub"))
                {
                    Logger.Log($"the payload contains a sub {payload["sub"]}", writer);
                }
                else
                {
                    Logger.Log("the payload doesn't contain sub", writer);
                }
            }
        }

        private static async Task RpUserInfoBadSubClaim()
        {
            using (var writer = File.AppendText(LogPath + "rp-userinfo-bad-sub-claim.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-userinfo-bad-sub-claim/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an access token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(token.IdToken);
                var idSub = payload["sub"].ToString();
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, token.AccessToken);
                var userInfoSub = userInfo.Value<string>("sub");
                if (userInfoSub == idSub)
                {
                    Logger.Log("subject are equals", writer);
                }
                else
                {
                    Logger.Log($"the subject are not equals {idSub} != {userInfoSub}", writer);
                }
            }
        }

        private static async Task RpUserInfoBearerBody()
        {
            using (var writer = File.AppendText(LogPath + "rp-userinfo-bearer-body.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-userinfo-bearer-body/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an access token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, token.AccessToken, true);
                Logger.Log("user information has been returned", writer);
            }
        }

        private static async Task RpUserInfoBearerHeader()
        {
            using (var writer = File.AppendText(LogPath + "rp-userinfo-bearer-header.log"))
            {
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                var identityServerClientFactory = new IdentityServerClientFactory();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-userinfo-bearer-header/.well-known/openid-configuration");
                Logger.Log("Register the client", writer);
                var client = await identityServerClientFactory.CreateRegistrationClient()
                    .ExecuteAsync(new Core.Common.DTOs.Client
                    {
                        RedirectUris = new List<string>
                        {
                            Constants.RedirectUriCode
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
                Logger.Log("Get an authorization code", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = "code",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var code = auth.Content.Value<string>("code");
                Logger.Log("Get an access token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, token.AccessToken, false);
                Logger.Log("user information has been returned", writer);
            }
        }
    }
}
