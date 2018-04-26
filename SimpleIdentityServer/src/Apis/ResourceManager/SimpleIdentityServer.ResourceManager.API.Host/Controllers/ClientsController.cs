using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using SimpleIdentityServer.ResourceManager.Core.Exceptions;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using System;
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
        private readonly IEndpointHelper _endpointHelper;

        public ClientsController(IEndpointRepository endpointRepository, ITokenStore tokenStore,
            IOpenIdManagerClientFactory openIdManagerClientFactory, IEndpointHelper endpointHelper)
        {
            _endpointRepository = endpointRepository;
            _tokenStore = tokenStore;
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _endpointHelper = endpointHelper;
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

        [HttpPost("openid/.search/{url?}")]
        public Task<IActionResult> SearchOpenidClients([FromBody] SearchClientsRequest request, string url)
        {
            return SearchClients(request, EndpointTypes.OPENID, url);
        }

        [HttpPost("openid/{url?}")]
        public Task<IActionResult> AddOpenidClient([FromBody] ClientResponse request, string url)
        {
            return AddClient(request, EndpointTypes.OPENID, url);
        }

        [HttpPut("openid/{url?}")]
        public Task<IActionResult> UpdateOpenidClient([FromBody] UpdateClientRequest request, string url)
        {
            return UpdateClient(request, EndpointTypes.OPENID, url);
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

        [HttpPost("auth/.search/{url?}")]
        public Task<IActionResult> SearchAuthClients([FromBody] SearchClientsRequest request, string url)
        {
            return SearchClients(request, EndpointTypes.AUTH, url);
        }

        [HttpPost("auth/{url?}")]
        public Task<IActionResult> AddAuthClient([FromBody] ClientResponse request, string url)
        {
            return AddClient(request, EndpointTypes.AUTH, url);
        }

        [HttpPut("openid/{url?}")]
        public Task<IActionResult> UpdateAuthdClient([FromBody] UpdateClientRequest request, string url)
        {
            return UpdateClient(request, EndpointTypes.AUTH, url);
        }

        private async Task<IActionResult> GetClient(string id, string url, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            EndpointAggregate endpoint;
            try
            {
                endpoint = await _endpointHelper.TryGetEndpoint(url, type);
            }
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }

            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { _scopeName });
            var client = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveGet(new Uri(endpoint.ManagerUrl), id, grantedToken.AccessToken);
            if (client == null || client.ContainsError)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(JsonConvert.SerializeObject(client.Content).ToString());
        }

        private async Task<IActionResult> DeleteClient(string id, string url, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }


            EndpointAggregate endpoint;
            try
            {
                endpoint = await _endpointHelper.TryGetEndpoint(url, type);
            }
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }

            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { _scopeName });
            var result = await _openIdManagerClientFactory.GetOpenIdsClient().ResolvedDelete(new Uri(endpoint.ManagerUrl), id, grantedToken.AccessToken);
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

                return new JsonResult(JsonConvert.SerializeObject(error))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }
            
            return new OkResult();
        }

        private async Task<IActionResult> SearchClients(SearchClientsRequest parameter, EndpointTypes type, string url)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            EndpointAggregate endpoint;
            try
            {
                endpoint = await _endpointHelper.TryGetEndpoint(url, type);
            }
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }

            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { _scopeName });
            var result = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveSearch(new Uri(endpoint.ManagerUrl), parameter, grantedToken.AccessToken);
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

                return new JsonResult(JsonConvert.SerializeObject(error))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return new OkObjectResult(result.Content);
        }

        private async Task<IActionResult> AddClient(ClientResponse parameter, EndpointTypes type, string url)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            EndpointAggregate endpoint;
            try
            {
                endpoint = await _endpointHelper.TryGetEndpoint(url, type);
            }
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
            
            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { _scopeName });
            var result = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveAdd(new Uri(endpoint.ManagerUrl), parameter, grantedToken.AccessToken);
            if (result.ContainsError)
            {
                var error = result.Error;
                if (error == null)
                {
                    error = new ErrorResponse
                    {
                        Code = Constants.ErrorCodes.InternalError,
                        Message = Constants.Errors.ErrInsertClient
                    };
                }

                return new JsonResult(JsonConvert.SerializeObject(error))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return new OkResult();
        }

        private async Task<IActionResult> UpdateClient(UpdateClientRequest parameter, EndpointTypes type, string url)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            EndpointAggregate endpoint;
            try
            {
                endpoint = await _endpointHelper.TryGetEndpoint(url, type);
            }
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
            
            var grantedToken = await _tokenStore.GetToken(endpoint.AuthUrl, endpoint.ClientId, endpoint.ClientSecret, new[] { _scopeName });
            var result = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveUpdate(new Uri(endpoint.ManagerUrl), parameter, grantedToken.AccessToken);
            if (result.ContainsError)
            {
                var error = result.Error;
                if (error == null)
                {
                    error = new ErrorResponse
                    {
                        Code = Constants.ErrorCodes.InternalError,
                        Message = Constants.Errors.ErrUpdateClient
                    };
                }

                return new JsonResult(JsonConvert.SerializeObject(error))
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };
            }

            return new OkResult();
        }
    }
}
