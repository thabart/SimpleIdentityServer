using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Client.DTOs.Parameters;
using SimpleIdentityServer.Manager.Client.DTOs.Responses;
using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using SimpleIdentityServer.ResourceManager.API.Host.Stores;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ClientsController)]
    public class ClientsController : Controller
    {
        private readonly IEndpointRepository _endpointRepository;
        private readonly ITokenStore _tokenStore;
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;

        public ClientsController(IEndpointRepository endpointRepository, ITokenStore tokenStore,
            IOpenIdManagerClientFactory openIdManagerClientFactory)
        {
            _endpointRepository = endpointRepository;
            _tokenStore = tokenStore;
            _openIdManagerClientFactory = openIdManagerClientFactory;
        }

        [HttpGet("openid/{id}/{url?}")]
        public async Task<IActionResult> GetOpenidClient(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var endpoint = await GetEndpoint(url, EndpointTypes.OPENID);
            if (endpoint == null)
            {
                return this.GetError(Constants.Errors.ErrNoEndpoint, HttpStatusCode.InternalServerError);
            }

            if (string.IsNullOrWhiteSpace(endpoint.AuthUrl) || string.IsNullOrWhiteSpace(endpoint.ClientId) || string.IsNullOrWhiteSpace(endpoint.ClientSecret))
            {
                return this.GetError(Constants.Errors.ErrAuthNotConfigured, HttpStatusCode.InternalServerError);
            }

            if (string.IsNullOrWhiteSpace(endpoint.ManagerUrl))
            {
                return this.GetError(Constants.Errors.ErrManagerApiNotConfigured, HttpStatusCode.InternalServerError);
            }

            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { "manager" });
            var client = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveGet(new Uri(endpoint.ManagerUrl), id, grantedToken.AccessToken);
            if (client == null || client.ContainsError)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(ToJson(client));
        }

        [HttpDelete("openid/{id}/{url?}")]
        public async Task<IActionResult> DeleteOpenidClient(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;

        }

        [HttpPost("openid/.search")]
        public async Task<IActionResult> SearchOpenidClients([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var parameter = ToSearchParameter(jObj);
            return null;
        }

        [HttpGet("auth/{id}/{url?}")]
        public async Task<IActionResult> GetAuthClient(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;
        }

        [HttpDelete("auth/{id}/{url?}")]
        public async Task<IActionResult> DeleteAuthClient(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return null;

        }

        [HttpPost("auth/.search")]
        public async Task<IActionResult> SearchAuthClients([FromBody] JObject jObj)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            return null;
        }

        private async Task<EndpointAggregate> GetEndpoint(string url, EndpointTypes type)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                var endpoint = await _endpointRepository.Get(url);
                return endpoint;
            }

            var endpoints = await _endpointRepository.Search(new SearchEndpointsParameter
            {
                Type = type
            });
            if (endpoints == null || !endpoints.Any())
            {
                return null;
            }

            var minOrder = endpoints.Min(e => e.Order);
            return endpoints.First(e => e.Order == minOrder);
        }

        private static JObject ToJson(OpenIdClientResponse client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var jObj = new JObject();
            jObj.Add(Constants.ClientNames.ClientId, client.ClientId);
            jObj.Add(Constants.ClientNames.ClientName, client.ClientName);
            jObj.Add(Constants.ClientNames.LogoUri, client.LogoUri);
            return jObj;
        }

        private static SearchClientParameter ToSearchParameter(JObject jObj)
        {
            var parameter = new SearchClientParameter();
            JToken jStartIndex,
                jCount,
                jClientIds,
                jClientNames;
            if (jObj.TryGetValue(Constants.SearchClientNames.ClientNames, out jClientNames))
            {
                var jArrClientNames = jClientNames as JArray;
                if (jArrClientNames != null)
                {
                    var names = new List<string>();
                    foreach (var name in jArrClientNames)
                    {
                        names.Add(name.ToString());
                    }

                    parameter.ClientNames = names;
                }
            }

            if (jObj.TryGetValue(Constants.SearchClientNames.ClientIds, out jClientIds))
            {
                var jArrClientIds = jClientIds as JArray;
                if (jArrClientIds != null)
                {
                    var ids = new List<string>();
                    foreach (var id in jArrClientIds)
                    {
                        ids.Add(id.ToString());
                    }

                    parameter.ClientIds = ids;
                }
            }

            if (jObj.TryGetValue(Constants.SearchNames.StartIndex, out jStartIndex))
            {
                int startIndex;
                if (int.TryParse(jStartIndex.ToString(), out startIndex))
                {
                    parameter.StartIndex = startIndex;
                }
            }

            if (jObj.TryGetValue(Constants.SearchNames.Count, out jCount))
            {
                int count;
                if (int.TryParse(jCount.ToString(), out count))
                {
                    parameter.Count = count;
                }
            }

            return parameter;
        }
    }
}
