using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
