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
    [Route(Constants.RouteNames.ResourceOwnersController)]
    public class ResourceOwnersController : Controller
    {
        private const string _scopeName = "manager";
        private readonly IEndpointRepository _endpointRepository;
        private readonly ITokenStore _tokenStore;
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IEndpointHelper _endpointHelper;

        public ResourceOwnersController(IEndpointRepository endpointRepository, ITokenStore tokenStore,
            IOpenIdManagerClientFactory openIdManagerClientFactory, IEndpointHelper endpointHelper)
        {
            _endpointRepository = endpointRepository;
            _tokenStore = tokenStore;
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _endpointHelper = endpointHelper;
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

            EndpointAggregate endpoint;
            try
            {
                endpoint = await _endpointHelper.TryGetEndpoint(url, EndpointTypes.OPENID);
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
            var client = await _openIdManagerClientFactory.GetResourceOwnerClient().ResolveGet(new Uri(endpoint.ManagerUrl), id, grantedToken.AccessToken);
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

            EndpointAggregate endpoint;
            try
            {
                endpoint = await _endpointHelper.TryGetEndpoint(url, EndpointTypes.OPENID);
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
            var result = await _openIdManagerClientFactory.GetResourceOwnerClient().ResolvedDelete(new Uri(endpoint.ManagerUrl), id, grantedToken.AccessToken);
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

            EndpointAggregate endpoint;
            try
            {
                endpoint = await _endpointHelper.TryGetEndpoint(url, EndpointTypes.OPENID);
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
            var result = await _openIdManagerClientFactory.GetResourceOwnerClient().ResolveSearch(new Uri(endpoint.ManagerUrl), parameter, grantedToken.AccessToken);
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
    }
}
