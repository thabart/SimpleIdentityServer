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
        private const string _scopeName = "manager";
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
        public Task<IActionResult> GetOpenidClient(string id, string url)
        {
            return GetClient(id, url, EndpointTypes.OPENID);
        }

        [HttpDelete("openid/{id}/{url?}")]
        public Task<IActionResult> DeleteOpenidClient(string id, string url)
        {
            return DeleteClient(id, url, EndpointTypes.OPENID);
        }

        [HttpPost("openid/.search")]
        public Task<IActionResult> SearchOpenidClients([FromBody] JObject jObj)
        {
            return SearchClients(jObj, EndpointTypes.OPENID);
        }

        [HttpGet("auth/{id}/{url?}")]
        public Task<IActionResult> GetAuthClient(string id, string url)
        {
            return GetClient(id, url, EndpointTypes.AUTH);
        }

        [HttpDelete("auth/{id}/{url?}")]
        public Task<IActionResult> DeleteAuthClient(string id, string url)
        {
            return DeleteClient(id, url, EndpointTypes.AUTH);
        }

        [HttpPost("auth/.search")]
        public Task<IActionResult> SearchAuthClients([FromBody] JObject jObj)
        {
            return SearchClients(jObj, EndpointTypes.AUTH);
        }

        private async Task<IActionResult> GetClient(string id, string url, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var endpoint = await TryGetEndpoint(url, type);
            if (endpoint.Value != null)
            {
                return endpoint.Value;
            }

            var edp = endpoint.Key;
            var grantedToken = await _tokenStore.GetToken(edp.AuthUrl, edp.ClientId, edp.ClientSecret, new[] { _scopeName });
            var client = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveGet(new Uri(edp.ManagerUrl), id, grantedToken.AccessToken);
            if (client == null || client.ContainsError)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(ToJson(client));
        }

        private async Task<IActionResult> DeleteClient(string id, string url, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var endpoint = await TryGetEndpoint(url, type);
            if (endpoint.Value != null)
            {
                return endpoint.Value;
            }

            var edp = endpoint.Key;
            var grantedToken = await _tokenStore.GetToken(edp.AuthUrl, edp.ClientId, edp.ClientSecret, new[] { _scopeName });
            var result = await _openIdManagerClientFactory.GetOpenIdsClient().ResolvedDelete(new Uri(edp.ManagerUrl), id, grantedToken.AccessToken);
            if (result.ContainsError)
            {
                var error = result.Error;
                if (error == null)
                {
                    error = new ErrorResponse
                    {
                        Code = Constants.ErrorCodes.InternalError,
                        Message = Constants.Errors.ErrDeleteClient
                    };
                }

                return new JsonResult(ToJson(error))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return new OkObjectResult(result);
        }

        private async Task<IActionResult> SearchClients([FromBody] JObject jObj, EndpointTypes type)
        {
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var parameter = ToSearchParameter(jObj);
            var endpoint = await TryGetEndpoint(parameter.Url, type);
            if (endpoint.Value != null)
            {
                return endpoint.Value;
            }

            var edp = endpoint.Key;
            var grantedToken = await _tokenStore.GetToken(edp.AuthUrl, edp.ClientId, edp.ClientSecret, new[] { _scopeName });
            var result = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveSearch(new Uri(edp.ManagerUrl), parameter, grantedToken.AccessToken);
            if (result.ContainsError)
            {
                var error = result.Error;
                if (error == null)
                {
                    error = new ErrorResponse
                    {
                        Code = Constants.ErrorCodes.InternalError,
                        Message = Constants.Errors.ErrSearchClients
                    };
                }

                return new JsonResult(ToJson(error))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return new OkObjectResult(result);
        }

        private async Task<KeyValuePair<EndpointAggregate, IActionResult>> TryGetEndpoint(string url, EndpointTypes type)
        {
            var endpoint = await GetEndpoint(url, EndpointTypes.OPENID);
            if (endpoint == null)
            {
                return new KeyValuePair<EndpointAggregate, IActionResult>(null, this.GetError(Constants.Errors.ErrNoEndpoint, HttpStatusCode.InternalServerError));
            }

            if (string.IsNullOrWhiteSpace(endpoint.AuthUrl) || string.IsNullOrWhiteSpace(endpoint.ClientId) || string.IsNullOrWhiteSpace(endpoint.ClientSecret))
            {
                return new KeyValuePair<EndpointAggregate, IActionResult>(null, this.GetError(Constants.Errors.ErrAuthNotConfigured, HttpStatusCode.InternalServerError));
            }

            if (string.IsNullOrWhiteSpace(endpoint.ManagerUrl))
            {
                return new KeyValuePair<EndpointAggregate, IActionResult>(null, this.GetError(Constants.Errors.ErrManagerApiNotConfigured, HttpStatusCode.InternalServerError));
            }

            return new KeyValuePair<EndpointAggregate, IActionResult>(endpoint, null);
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

        private static JObject ToJson(ErrorResponse errorResponse)
        {
            if (errorResponse == null)
            {
                throw new ArgumentNullException(nameof(errorResponse));
            }

            var jObj = new JObject();
            jObj.Add(Constants.ErrorDtoNames.Code, errorResponse.Code);
            jObj.Add(Constants.ErrorDtoNames.Message, errorResponse.Message);
            return jObj;
        }

        private static JObject ToJson(SearchClientResponse searchClientResponse)
        {
            if (searchClientResponse == null)
            {
                throw new ArgumentNullException(nameof(searchClientResponse));
            }

            var jObj = new JObject();
            var clients = new JArray();
            if (searchClientResponse.Content != null)
            {
                foreach(var client in searchClientResponse.Content)
                {
                    clients.Add(ToJson(client));
                }
            }

            jObj.Add(Constants.SearchNames.Content, clients);
            jObj.Add(Constants.SearchNames.StartIndex, searchClientResponse.StartIndex);
            jObj.Add(Constants.SearchNames.Count, searchClientResponse.NbResults);
            return jObj;
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
            if (jObj == null)
            {
                throw new ArgumentNullException(nameof(jObj));
            }

            var parameter = new SearchClientParameter();
            JToken jStartIndex,
                jCount,
                jClientIds,
                jClientNames,
                jUrl;
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

            if (jObj.TryGetValue(Constants.SearchNames.Url, out jUrl))
            {
                parameter.Url = jUrl.ToString();
            }

            return parameter;
        }
    }
}
