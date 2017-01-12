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
        private static string SubIdTokenPath = @"\id_token\";
        private static string SubIdTokenTokenPath = @"\id_token+token\";

        public static async Task Start()
        {
            _jwsParser = new JwsParser(null);
            _jsonWebKeyConverter = new JsonWebKeyConverter();
            // id_token
            await RpResponseTypeIdToken();
            await RpScopeUserInfoClaims(SubIdTokenPath, new string[] { "id_token" });
            await RpNonceUnlessCodeFlow(SubIdTokenPath, new string[] { "id_token" });
            await RpNonceInvalid(SubIdTokenPath, new string[] { "id_token" });
            await RpIdTokenAud(SubIdTokenPath, new string[] { "id_token" });
            await RpIdTokenKidAbsentSingleJwks(SubIdTokenPath, new string[] { "id_token" });
            await RpIdTokenIssuerMismatch(SubIdTokenPath, new string[] { "id_token" });
            await RpIdTokenKidAbsentMultipleJwks(SubIdTokenPath, new string[] { "id_token" });
            await RpIdTokenBadSigRS256(SubIdTokenPath, new string[] { "id_token" });
            await RpIdTokenIat(SubIdTokenPath, new string[] { "id_token" });
            await RpIdTokenSigRS256(SubIdTokenPath, new string[] { "id_token" });
            await RpIdTokenSub(SubIdTokenPath, new string[] { "id_token" });
            // id_token+token
            await RpResponseTypeIdTokenToken();
            await RpScopeUserInfoClaims(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpNonceUnlessCodeFlow(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpNonceInvalid(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpIdTokenAud(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpIdTokenKidAbsentSingleJwks(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpIdTokenIssuerMismatch(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpIdTokenBadAtHash();
            await RpIdTokenKidAbsentMultipleJwks(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpIdTokenBadSigRS256(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpIdTokenIat(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpIdTokenSigRS256(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpIdTokenSub(SubIdTokenTokenPath, new string[] { "id_token", "token" });
            await RpUserInfoBadSubClaim();
            await RpUserInfoBearerBody();
            await RpUserInfoBearerHeader();
        }

        private static async Task RpResponseTypeIdToken()
        {
            using (var writer = File.AppendText(LogPath + SubIdTokenPath + "rp-response_type-id_token.log"))
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

        private static async Task RpScopeUserInfoClaims(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-scope-userinfo-claims.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
                            Scope = "openid email profile",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
                Logger.Log($"the email is {payload["email"]}", writer);
            }
        }

        private static async Task RpNonceUnlessCodeFlow(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-nonce-unless-code-flow.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpNonceInvalid(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-nonce-invalid.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpIdTokenAud(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-aud.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpIdTokenKidAbsentSingleJwks(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-kid-absent-single-jwks.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpIdTokenIssuerMismatch(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-issuer-mismatch.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpIdTokenKidAbsentMultipleJwks(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-kid-absent-multiple-jwks.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpIdTokenBadSigRS256(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-bad-sig-rs256.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpIdTokenIat(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-iat.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpIdTokenSigRS256(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-sig-rs256.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpIdTokenSub(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-sub.log"))
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
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var result = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
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

        private static async Task RpResponseTypeIdTokenToken()
        {
            using (var writer = File.AppendText(LogPath + SubIdTokenTokenPath + "rp-response_type-id_token+token.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-response_type-id_token+token/.well-known/openid-configuration");
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
                            "id_token",
                            "token"
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
                            ResponseType = "id_token token",
                            Scope = "openid",
                            Nonce = nonce
                        });
                Logger.Log($"IdToken {result.Content.Value<string>("id_token")} & access token {result.Content.Value<string>("access_token")} have been returned", writer);
            }
        }

        private static async Task RpIdTokenBadAtHash()
        {
            using (var writer = File.AppendText(LogPath + SubIdTokenTokenPath + "rp-id_token-bad-at_hash.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-bad-at_hash/.well-known/openid-configuration");
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
                            "id_token",
                            "token"
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
                            ResponseType = "id_token token",
                            Scope = "openid",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                Logger.Log($"IdToken {idToken} & access token {result.Content.Value<string>("access_token")} have been returned", writer);
                var payload = _jwsParser.GetPayload(idToken);
                if (!payload.ContainsKey("at_hash"))
                {
                    Logger.Log("the payload doesn't contain 'at_hash'", writer);
                }
                else
                {
                    var atHash = payload["at_hash"].ToString();
                    Logger.Log($"the hash {atHash} is not valid", writer);
                }
            }
        }

        private static async Task RpUserInfoBadSubClaim()
        {
            using (var writer = File.AppendText(LogPath + SubIdTokenTokenPath + "rp-userinfo-bad-sub-claim.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-userinfo-bad-sub-claim/.well-known/openid-configuration");
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
                            "id_token",
                            "token"
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
                            ResponseType = "id_token token",
                            Scope = "openid",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var accessToken = result.Content.Value<string>("access_token");
                Logger.Log($"IdToken {idToken} & access token {result.Content.Value<string>("access_token")} have been returned", writer);
                var payload = _jwsParser.GetPayload(idToken);
                if (!payload.ContainsKey("sub"))
                {
                    Logger.Log("the identity token doesn't contain subject", writer);
                }
                else
                {
                    var sub = payload["sub"].ToString();
                    var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                        .GetUserInfoAsync(discovery.UserInfoEndPoint, accessToken);
                    var userInfoSub = userInfo.Value<string>("sub");
                    if (userInfoSub ==  sub)
                    {
                        Logger.Log("the subject is correct", writer);
                    }
                    else
                    {
                        Logger.Log("the subject is not correct", writer);
                    }
                }
            }
        }

        private static async Task RpUserInfoBearerBody()
        {
            using (var writer = File.AppendText(LogPath + SubIdTokenTokenPath + "rp-userinfo-bearer-body.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-userinfo-bearer-body/.well-known/openid-configuration");
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
                            "id_token",
                            "token"
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
                            ResponseType = "id_token token",
                            Scope = "openid",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var accessToken = result.Content.Value<string>("access_token");
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, accessToken, true);
                Logger.Log("user information has been returned", writer);
            }
        }

        private static async Task RpUserInfoBearerHeader()
        {
            using (var writer = File.AppendText(LogPath + SubIdTokenTokenPath + "rp-userinfo-bearer-header.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-userinfo-bearer-header/.well-known/openid-configuration");
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
                            "id_token",
                            "token"
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
                            ResponseType = "id_token token",
                            Scope = "openid",
                            Nonce = nonce
                        });
                var idToken = result.Content.Value<string>("id_token");
                var accessToken = result.Content.Value<string>("access_token");
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, accessToken, false);
                Logger.Log("user information has been returned", writer);
            }
        }
    }
}
