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
    public class Hybrid
    {
        private static IJwsParser _jwsParser;
        private static IJsonWebKeyConverter _jsonWebKeyConverter;
        private static string LogPath = @"C:\Users\thabart\Desktop\Logs\Hybrid";
        private static string SubCodeIdTokenPath = @"\code+id_token\";
        private static string SubCodeIdTokenTokenPath = @"\code+id_token+token\";
        private static string SubCodeTokenPath = @"\code+token\";

        public static async Task Start()
        {
            _jwsParser = new JwsParser(new CreateJwsSignature(new CngKeySerializer()));
            _jsonWebKeyConverter = new JsonWebKeyConverter();
            // code+id_token
            // await RpResponseTypeCodeIdToken();
            // await RpScopeUserInfoClaims(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpNonceUnlessCodeFlow(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpNonceInvalid(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpTokenEndpointClientSecretBasic(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenAud(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenKidAbsentSingleJwks(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenBadCHash(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenIssuerMismatch(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenKidAbsentMultipleJwks(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenBadSigRS256(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenIat(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenSigRS256(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpIdTokenSub(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpUserInfoBadSubClaim(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpUserInfoBearerBody(SubCodeIdTokenPath, new string[] { "id_token code" });
            // await RpUserInfoBearerHeader(SubCodeIdTokenPath, new string[] { "id_token code" });
            // code+id_token+token
            await RpResponseTypeCodeIdTokenToken();
            await RpScopeUserInfoClaims(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpNonceUnlessCodeFlow(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpNonceInvalid(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpTokenEndpointClientSecretBasic(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenAud(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenKidAbsentSingleJwks(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenBadCHash(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenIssuerMismatch(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenBadAtHash(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenKidAbsentMultipleJwks(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenBadSigRS256(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenIat(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpIdTokenSub(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpUserInfoBadSubClaim(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpUserInfoBearerBody(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
            await RpUserInfoBearerHeader(SubCodeIdTokenTokenPath, new string[] { "id_token token code" });
        }

        private static async Task RpResponseTypeCodeIdToken()
        {
            using (var writer = File.AppendText(LogPath + SubCodeIdTokenPath + "rp-response_type-code+id_token.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-response_type-code+id_token/.well-known/openid-configuration");
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
                            "implicit",
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token code"
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
                            ResponseType = "id_token code",
                            Scope = "openid",
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
                        });
                Logger.Log($"IdToken {result.Content.Value<string>("id_token")} & code {result.Content.Value<string>("code")} has been returned", writer);
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
                        });
                var idToken = result.Content.Value<string>("id_token");
                var payload = _jwsParser.GetPayload(idToken);
                Logger.Log($"the email is {payload["sub"]}", writer);
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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

        private static async Task RpTokenEndpointClientSecretBasic(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-token_endpoint-client_secret_basic.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-token_endpoint-client_secret_basic/.well-known/openid-configuration");
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
                            "implicit",
                            "authorization_code"
                        },
                        TokenEndpointAuthMethod = "client_secret_basic",
                        ResponseTypes = responseTypes
                    }, discovery.RegistrationEndPoint);
                Logger.Log("Get authorization", writer);
                var auth = await identityServerClientFactory.CreateAuthorizationClient()
                    .ExecuteAsync(discovery.AuthorizationEndPoint,
                        new Core.Common.DTOs.AuthorizationRequest
                        {
                            ClientId = client.ClientId,
                            State = state,
                            RedirectUri = Constants.RedirectUriCode,
                            ResponseType = string.Join(" ", responseTypes),
                            Scope = "openid email profile",
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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

        private static async Task RpIdTokenBadCHash(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-bad-c_hash.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-id_token-bad-c_hash/.well-known/openid-configuration");
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
                            "implicit",
                            "authorization_code"
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
                            Scope = "openid",
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
                        });
                var idToken = result.Content.Value<string>("id_token");
                Logger.Log($"IdToken {idToken} & access token {result.Content.Value<string>("access_token")} have been returned", writer);
                var payload = _jwsParser.GetPayload(idToken);
                if (!payload.ContainsKey("c_hash"))
                {
                    Logger.Log("the payload doesn't contain 'c_hash'", writer);
                }
                else
                {
                    var cHash = payload["c_hash"].ToString();
                    Logger.Log($"the c_hash {cHash} is not valid", writer);
                }
            }
        }

        private static async Task RpIdTokenBadAtHash(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-id_token-bad-at_hash.log"))
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
                            "implicit",
                            "authorization_code"
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
                            Scope = "openid",
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                    Logger.Log($"the at_hash {atHash} is not valid", writer);
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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
                            "implicit",
                            "authorization_code"
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
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
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

        private static async Task RpUserInfoBadSubClaim(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-userinfo-bad-sub-claim.log"))
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
                            "implicit",
                            "authorization_code"
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
                            Scope = "openid",
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
                        });
                var idToken = result.Content.Value<string>("id_token");
                var code = result.Content.Value<string>("code");
                Logger.Log($"IdToken {idToken} & code {result.Content.Value<string>("code")} have been returned", writer);
                Logger.Log("Get access token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(idToken);
                if (!payload.ContainsKey("sub"))
                {
                    Logger.Log("the identity token doesn't contain subject", writer);
                }
                else
                {
                    var sub = payload["sub"].ToString();
                    var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                        .GetUserInfoAsync(discovery.UserInfoEndPoint, token.AccessToken);
                    var userInfoSub = userInfo.Value<string>("sub");
                    if (userInfoSub == sub)
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

        private static async Task RpUserInfoBearerBody(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-userinfo-bearer-body.log"))
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
                            "implicit",
                            "authorization_code"
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
                            Scope = "openid",
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
                        });
                var idToken = result.Content.Value<string>("id_token");
                var code = result.Content.Value<string>("code");
                Logger.Log("Get access token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var payload = _jwsParser.GetPayload(idToken);
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, token.AccessToken, true);
                Logger.Log("user information has been returned", writer);
            }
        }

        private static async Task RpUserInfoBearerHeader(string subPath, IEnumerable<string> responseTypes)
        {
            using (var writer = File.AppendText(LogPath + subPath + "rp-userinfo-bearer-header.log"))
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
                            "implicit",
                            "authorization_code"
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
                            Scope = "openid",
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
                        });
                var idToken = result.Content.Value<string>("id_token");
                var code = result.Content.Value<string>("code");
                Logger.Log("Get access token", writer);
                var token = await identityServerClientFactory.CreateAuthSelector()
                    .UseClientSecretBasicAuth(client.ClientId, client.ClientSecret)
                    .UseAuthorizationCode(code, Constants.RedirectUriCode)
                    .ExecuteAsync(discovery.TokenEndPoint);
                var userInfo = await identityServerClientFactory.CreateUserInfoClient()
                    .GetUserInfoAsync(discovery.UserInfoEndPoint, token.AccessToken, false);
                Logger.Log("user information has been returned", writer);
            }
        }

        private static async Task RpResponseTypeCodeIdTokenToken()
        {
            using (var writer = File.AppendText(LogPath + SubCodeIdTokenTokenPath + "rp-response_type-code+id_token+token.log"))
            {
                var identityServerClientFactory = new IdentityServerClientFactory();
                var state = Guid.NewGuid().ToString();
                var nonce = Guid.NewGuid().ToString();
                Logger.Log("Call OpenIdConfiguration", writer);
                var discovery = await identityServerClientFactory.CreateDiscoveryClient()
                    .GetDiscoveryInformationAsync(Constants.BaseUrl + "/rp-response_type-code+id_token+token/.well-known/openid-configuration");
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
                            "implicit",
                            "authorization_code"
                        },
                        ResponseTypes = new List<string>
                        {
                            "id_token code token"
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
                            ResponseType = "id_token code token",
                            Scope = "openid",
                            Nonce = nonce,
                            ResponseMode = Core.Common.DTOs.ResponseModes.FormPost
                        });
                Logger.Log($"IdToken {result.Content.Value<string>("id_token")} & code {result.Content.Value<string>("code")} & token {result.Content.Value<string>("access_token")} have been returned", writer);
            }
        }
    }
}
