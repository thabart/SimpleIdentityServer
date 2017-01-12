using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Client.Test
{
    public class Implicit
    {
        private static IJwsParser _jwsParser;
        private static IJsonWebKeyConverter _jsonWebKeyConverter;
        private static string LogPath = @"C:\Users\thabart\Desktop\Logger.Logs\Implicit";

        public static async Task Start()
        {
            _jwsParser = new JwsParser(null);
            _jsonWebKeyConverter = new JsonWebKeyConverter();
            await RpResponseTypeIdToken();
            await RpScopeUserInfoClaims();
            await RpNonceUnlessCodeFlow();
            await RpNonceInvalid();
            await RpIdTokenAud();
            await RpIdTokenKidAbsentSingleJwks();
            await RpIdTokenIssuerMismatch();
            await RpIdTokenKidAbsentMultipleJwks();
            await RpIdTokenBadSigRS256();
            await RpIdTokenIat();
            await RpIdTokenSigRS256();
            await RpIdTokenSub();
        }

        private static async Task RpResponseTypeIdToken()
        {
            using (var writer = File.AppendText(LogPath + "rp-response_type-id_token.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-response_type-id_token/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid",
                            Nonce = nonce
                        });
                Logger.Log($"IdToken has been returned {result.Content.Value<string>("id_token")}", writer);
            }
        }

        private static async Task RpScopeUserInfoClaims()
        {
            using (var writer = File.AppendText(LogPath + "rp-scope-userinfo-claims.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-scope-userinfo-claims/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
                Logger.Log($"the email is {payload["email"]}", writer);
            }
        }

        private static async Task RpNonceUnlessCodeFlow()
        {
            using (var writer = File.AppendText(LogPath + "rp-nonce-unless-code-flow.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-nonce-unless-code-flow/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
                if (payload.Nonce == nonce)
                {
                    Logger.Log($"the id_token nonce & auth_nonce are equals", writer);
                }
                else
                {
                    Logger.Log("the nonce are not equals", writer);
                }
            }
        }

        private static async Task RpNonceInvalid()
        {
            using (var writer = File.AppendText(LogPath + "rp-nonce-invalid.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-nonce-invalid/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
                if (payload.Nonce == nonce)
                {
                    Logger.Log($"the id_token nonce & auth_nonce are equals", writer);
                }
                else
                {
                    Logger.Log("the nonce are not equals", writer);
                }
            }
        }

        private static async Task RpIdTokenAud()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-aud.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-aud/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
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
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-kid-absent-single-jwks/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var header = _jwsParser.GetHeader(idToken);
                Logger.Log($"receive the identity token {idToken}", writer);
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
                        var jwsPayload = _jwsParser.ValidateSignature(idToken, key);
                        if (jwsPayload == null)
                        {
                            Logger.Log("the json web key is not correct", writer);
                        }
                        else
                        {
                            Logger.Log($"identity token is correct {idToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenIssuerMismatch()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-issuer-mismatch.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-issuer-mismatch/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
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
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-kid-absent-multiple-jwks/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var header = _jwsParser.GetHeader(idToken);
                Logger.Log($"receive the identity token {idToken}", writer);
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
                        var jwsPayload = _jwsParser.ValidateSignature(idToken, key);
                        if (jwsPayload == null)
                        {
                            Logger.Log("the json web key is not correct", writer);
                        }
                        else
                        {
                            Logger.Log($"identity token is correct {idToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenBadSigRS256()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-bad-sig-rs256.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-bad-sig-rs256/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var header = _jwsParser.GetHeader(idToken);
                Logger.Log($"receive the identity token {idToken}", writer);
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
                        var jwsPayload = _jwsParser.ValidateSignature(idToken, key);
                        if (jwsPayload == null)
                        {
                            Logger.Log("the id_token signature is not correct", writer);
                        }
                        else
                        {
                            Logger.Log($"identity token is correct {idToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenIat()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-iat.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-iat/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
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

        private static async Task RpIdTokenSigRS256()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-sig-rs256.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-sig-rs256/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var header = _jwsParser.GetHeader(idToken);
                Logger.Log($"receive the identity token {idToken}", writer);
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
                        var jwsPayload = _jwsParser.ValidateSignature(idToken, key);
                        if (jwsPayload == null)
                        {
                            Logger.Log("the id_token signature is not correct", writer);
                        }
                        else
                        {
                            Logger.Log($"identity token is correct {idToken}", writer);
                        }
                    }
                }
            }
        }

        private static async Task RpIdTokenSub()
        {
            using (var writer = File.AppendText(LogPath + "rp-id_token-sub.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-sub/.well-known/openid-configuration");
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
                            "implicit"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token"
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
                            ResponseType = "id_token",
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
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
    }
}
