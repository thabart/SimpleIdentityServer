using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using SimpleIdentityServer.ResourceManager.Core.Exceptions;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ScopesController)]
    public class ScopesController : Controller
    {
        private const string _scopeName = "manager";
        private readonly IEndpointRepository _endpointRepository;
        private readonly ITokenStore _tokenStore;
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IEndpointHelper _endpointHelper;

        public ScopesController(IEndpointRepository endpointRepository, ITokenStore tokenStore,
               IOpenIdManagerClientFactory openIdManagerClientFactory, IEndpointHelper endpointHelper)
        {
            _endpointRepository = endpointRepository;
            _tokenStore = tokenStore;
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _endpointHelper = endpointHelper;
        }

        #region OPENID

        [HttpGet("openid/{id}/{url?}")]
        public Task<IActionResult> GetOpenIdScope(string id, string url)
        {
            return GetScope(id, url, EndpointTypes.OPENID);
        }

        [HttpGet("openid/{url?}")]
        public Task<IActionResult> GetAllOpenIdScope(string url)
        {
            return GetAllScopes(url, EndpointTypes.OPENID);
        }

        [HttpDelete("openid/{id}/{url?}")]
        public Task<IActionResult> DeleteOpenIdScope(string id, string url)
        {
            return DeleteScope(id, url, EndpointTypes.OPENID);
        }

        [HttpPost("openid/{url?}")]
        public Task<IActionResult> AddOpenIdScope([FromBody] ScopeResponse scopeResponse, string url)
        {
            return AddScope(scopeResponse, EndpointTypes.OPENID, url);
        }

        [HttpPut("openid/{url?}")]
        public Task<IActionResult> UpdateOpenIdScope([FromBody] ScopeResponse scopeResponse, string url)
        {
            return UpdateScope(scopeResponse, EndpointTypes.OPENID, url);
        }

        [HttpPost("openid/.search/{url?}")]
        public Task<IActionResult> SearchOpenIdScopes([FromBody] SearchScopesRequest request, string url)
        {
            return SearchScopes(request, EndpointTypes.OPENID, url);
        }

        #endregion

        #region AUTH

        [HttpGet("auth/{id}/{url?}")]
        public Task<IActionResult> GetAuthScope(string id, string url)
        {
            return GetScope(id, url, EndpointTypes.AUTH);
        }

        [HttpGet("auth/{url?}")]
        public Task<IActionResult> GetAllAuthScopes(string url)
        {
            return GetAllScopes(url, EndpointTypes.AUTH);
        }

        [HttpDelete("auth/{id}/{url?}")]
        public Task<IActionResult> DeleteAuthScope(string id, string url)
        {
            return DeleteScope(id, url, EndpointTypes.AUTH);
        }

        [HttpPost("auth/{url?}")]
        public Task<IActionResult> AddAuthScope([FromBody] ScopeResponse scopeResponse, string url)
        {
            return AddScope(scopeResponse, EndpointTypes.AUTH, url);
        }

        [HttpPut("auth/{url?}")]
        public Task<IActionResult> UpdateAuthScope([FromBody] ScopeResponse scopeResponse, string url)
        {
            return UpdateScope(scopeResponse, EndpointTypes.AUTH, url);
        }

        [HttpPost("auth/.search/{url?}")]
        public Task<IActionResult> SearchAuthScopes([FromBody] SearchScopesRequest request, string url)
        {
            return SearchScopes(request, EndpointTypes.AUTH, url);
        }

        #endregion

        public async Task<IActionResult> GetScope(string id, string url, EndpointTypes type)
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
            var client = await _openIdManagerClientFactory.GetScopesClient().ResolveGet(new Uri(endpoint.ManagerUrl), id, grantedToken.AccessToken);
            if (client == null || client.ContainsError)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(JsonConvert.SerializeObject(client.Content).ToString());
        }

        public async Task<IActionResult> GetAllScopes(string url, EndpointTypes type)
        {
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
            var result = await _openIdManagerClientFactory.GetScopesClient().ResolveGetAll(new Uri(endpoint.ManagerUrl), grantedToken.AccessToken);
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

        private async Task<IActionResult> DeleteScope(string id, string url, EndpointTypes type)
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
            var result = await _openIdManagerClientFactory.GetScopesClient().ResolvedDelete(new Uri(endpoint.ManagerUrl), id, grantedToken.AccessToken);
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

        private async Task<IActionResult> AddScope(ScopeResponse parameter, EndpointTypes type, string url)
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
            var result = await _openIdManagerClientFactory.GetScopesClient().ResolveAdd(new Uri(endpoint.ManagerUrl), parameter, grantedToken.AccessToken);
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

        private async Task<IActionResult> UpdateScope(ScopeResponse parameter, EndpointTypes type, string url)
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
            var result = await _openIdManagerClientFactory.GetScopesClient().ResolveUpdate(new Uri(endpoint.ManagerUrl), parameter, grantedToken.AccessToken);
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

        private async Task<IActionResult> SearchScopes(SearchScopesRequest parameter, EndpointTypes type, string url)
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
            var result = await _openIdManagerClientFactory.GetScopesClient().ResolveSearch(new Uri(endpoint.ManagerUrl), parameter, grantedToken.AccessToken);
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
    }
}
