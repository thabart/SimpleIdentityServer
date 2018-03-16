using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Uma.Common.DTOs;
using SimpleIdentityServer.Uma.ProtectedResource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.ProtectedResource.Controllers
{
    public class InformationsController : Controller
    {
        private const string AuthorizationName = "Authorization";
        private const string BearerName = "Bearer";
        private List<Information> _informations;

        private readonly IIdentityServerUmaClientFactory _identityServerUmaClientFactory;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;
        private readonly IJwsParser _jwsParser;

        public InformationsController(IIdentityServerUmaClientFactory identityServerUmaClientFactory, IIdentityServerClientFactory identityServerClientFactory,
            IJwsParser jwsParser)
        {
            _identityServerUmaClientFactory = identityServerUmaClientFactory;
            _identityServerClientFactory = identityServerClientFactory;
            _jwsParser = jwsParser;
            _informations = new List<Information>
            {
                new Information
                {
                    Id = "1",
                    Address = "adr bruxelles",
                    Gender = "M",
                    ResourceId = "1"
                }
            };
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var information = _informations.FirstOrDefault(i => i.Id == id);
            if (information == null)
            {
                return new NotFoundResult();
            }

            string accessToken;
            var grantedToken = await GetAccessToken(); // 1. Get an access token.
            if (!TryGetAccessToken(out accessToken)) // 2 Try to get the RPT tokens
            {
                var ticket = await _identityServerUmaClientFactory.GetPermissionClient() // 2.1 Get permission ticket.
                    .AddByResolution(new PostPermission
                    {
                        ResourceSetId = information.ResourceId,
                        Scopes = new[] { "read" }
                    }, "https://localhost:5445/.well-known/uma2-configuration", grantedToken.AccessToken);
                var ticketId = ticket.TicketId;
                var jObj = new JObject();
                jObj.Add("ticket_id", ticketId);
                return new OkObjectResult(jObj);
            }

            var introspectionResult = await _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth("resource_server", "resource_server")
                .Introspect(accessToken, TokenType.AccessToken)
                .ResolveAsync("https://localhost:5445/.well-known/uma2-configuration");
            if (!introspectionResult.Active)
            {
                return null;
            }

            var payload = _jwsParser.GetPayload(accessToken);
            if (!payload.ContainsKey("ticket"))
            {
                return null;
            }


            var ticketObj = JArray.Parse(payload["ticket"].ToString());
            // CHECK THE TICKET IS CORRECT.
            return null;
        }

        private bool TryGetAccessToken(out string accessToken)
        {
            accessToken = null;
            var headers = Request.Headers;
            if (headers.ContainsKey(AuthorizationName))
            {
                var authorizationValues = headers[AuthorizationName];
                accessToken = GetAccessToken(authorizationValues.First());
                return true;
            }

            return false;
        }

        private string GetAccessToken(string authorizationValue)
        {
            var splittedAuthorizationValue = authorizationValue.Split(' ');
            if (splittedAuthorizationValue.Count() == 2 &&
                splittedAuthorizationValue[0].Equals(BearerName, StringComparison.CurrentCultureIgnoreCase))
            {
                return splittedAuthorizationValue[1];
            }

            return string.Empty;
        }

        private async Task<GrantedToken> GetAccessToken()
        {
            return await _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth("resource_server", "resource_server")
                .UseClientCredentials("uma_protection")
                .ResolveAsync("https://localhost:5445/.well-known/uma2-configuration");
        }
    }
}
