using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
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
    [Route(Constants.RouteNames.ResourceOwnersController)]
    public class ResourceOwnersController : Controller
    {
        private const string _scopeName = "manager";
        private readonly IEndpointRepository _endpointRepository;
        private readonly ITokenStore _tokenStore;
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;

        public ResourceOwnersController(IEndpointRepository endpointRepository, ITokenStore tokenStore,
            IOpenIdManagerClientFactory openIdManagerClientFactory)
        {
            _endpointRepository = endpointRepository;
            _tokenStore = tokenStore;
            _openIdManagerClientFactory = openIdManagerClientFactory;
        }

        [HttpGet("{id}/{url?}")]
        public Task<IActionResult> Get(string id, string url)
        {
            return GetResourceOwner(id, url);
        }

        [HttpDelete("{id}/{url?}")]
        public Task<IActionResult> Delete(string id, string url)
        {
            return DeleteResourceOwner(id, url);
        }

        [HttpPost(".search/{url?}")]
        public Task<IActionResult> Search([FromBody] SearchResourceOwnersRequest request, string url)
        {
            return SearchResourceOwners(request, url);
        }
        
        private async Task<IActionResult> GetResourceOwner(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var endpoint = await TryGetEndpoint(url, EndpointTypes.OPENID);
            if (endpoint.Value != null)
            {
                return endpoint.Value;
            }

            var edp = endpoint.Key;
            var grantedToken = await _tokenStore.GetToken(edp.AuthUrl, edp.ClientId, edp.ClientSecret, new[] { _scopeName });
            var client = await _openIdManagerClientFactory.GetResourceOwnerClient().ResolveGet(new Uri(edp.ManagerUrl), id, grantedToken.AccessToken);
            if (client == null || client.ContainsError)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(JsonConvert.SerializeObject(client.Content).ToString());
        }

        private async Task<IActionResult> DeleteResourceOwner(string id, string url)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var endpoint = await TryGetEndpoint(url, EndpointTypes.OPENID);
            if (endpoint.Value != null)
            {
                return endpoint.Value;
            }

            var edp = endpoint.Key;
            var grantedToken = await _tokenStore.GetToken(edp.AuthUrl, edp.ClientId, edp.ClientSecret, new[] { _scopeName });
            var result = await _openIdManagerClientFactory.GetResourceOwnerClient().ResolvedDelete(new Uri(edp.ManagerUrl), id, grantedToken.AccessToken);
            if (result.ContainsError)
            {
                var error = result.Error;
                if (error == null)
                {
                    error = new ErrorResponse
                    {
                        Code = Constants.ErrorCodes.InternalError,
                        Message = Constants.Errors.ErrDeleteRo
                    };
                }

                return new JsonResult(JsonConvert.SerializeObject(error))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return new OkResult();
        }

        private async Task<IActionResult> SearchResourceOwners(SearchResourceOwnersRequest parameter, string url)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var endpoint = await TryGetEndpoint(url, EndpointTypes.OPENID);
            if (endpoint.Value != null)
            {
                return endpoint.Value;
            }

            var edp = endpoint.Key;
            var grantedToken = await _tokenStore.GetToken(edp.AuthUrl, edp.ClientId, edp.ClientSecret, new[] { _scopeName });
            var result = await _openIdManagerClientFactory.GetResourceOwnerClient().ResolveSearch(new Uri(edp.ManagerUrl), parameter, grantedToken.AccessToken);
            if (result.ContainsError)
            {
                var error = result.Error;
                if (error == null)
                {
                    error = new ErrorResponse
                    {
                        Code = Constants.ErrorCodes.InternalError,
                        Message = Constants.Errors.ErrSearchRos
                    };
                }

                return new JsonResult(JsonConvert.SerializeObject(error))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return new OkObjectResult(result.Content);
        }

        private async Task<KeyValuePair<EndpointAggregate, IActionResult>> TryGetEndpoint(string url, EndpointTypes type)
        {
            var endpoint = await GetEndpoint(url, type);
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
    }
}
