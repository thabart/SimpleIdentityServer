using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using SimpleIdentityServer.ResourceManager.Core.Api.Clients;
using SimpleIdentityServer.ResourceManager.Core.Exceptions;
using SimpleIdentityServer.ResourceManager.Core.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Stores;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ClientsController)]
    public class ClientsController : Controller
    {
        private readonly IClientActions _clientActions;
        private readonly ITokenStore _tokenStore;
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IEndpointHelper _endpointHelper;

        public ClientsController(IClientActions clientActions)
        {
            _clientActions = clientActions;
        }

        [Authorize("connected")]
        [HttpGet("openid/{id}")]
        public Task<IActionResult> GetOpenidClient(string id)
        {
            return GetClient(id, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpDelete("openid/{id}")]
        public Task<IActionResult> DeleteOpenidClient(string id)
        {
            return DeleteClient(id, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpPost("openid/.search")]
        public Task<IActionResult> SearchOpenidClients([FromBody] SearchClientsRequest request)
        {
            return SearchClients(request, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpPost("openid")]
        public Task<IActionResult> AddOpenidClient([FromBody] ClientResponse request)
        {
            return AddClient(request, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpPut("openid")]
        public Task<IActionResult> UpdateOpenidClient([FromBody] UpdateClientRequest request)
        {
            return UpdateClient(request, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpGet("auth/{id}")]
        public Task<IActionResult> GetAuthClient(string id)
        {
            return GetClient(id, EndpointTypes.AUTH);
        }

        [Authorize("connected")]
        [HttpDelete("auth/{id}")]
        public Task<IActionResult> DeleteAuthClient(string id)
        {
            return DeleteClient(id, EndpointTypes.AUTH);
        }

        [Authorize("connected")]
        [HttpPost("auth/.search")]
        public Task<IActionResult> SearchAuthClients([FromBody] SearchClientsRequest request)
        {
            return SearchClients(request, EndpointTypes.AUTH);
        }

        [Authorize("connected")]
        [HttpPost("auth")]
        public Task<IActionResult> AddAuthClient([FromBody] ClientResponse request)
        {
            return AddClient(request, EndpointTypes.AUTH);
        }

        [HttpPut("auth")]
        public Task<IActionResult> UpdateAuthdClient([FromBody] UpdateClientRequest request)
        {
            return UpdateClient(request, EndpointTypes.AUTH);
        }

        private async Task<IActionResult> GetClient(string id, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                var subject = User.GetSubject();
                var result = await _clientActions.Get(subject, id, type);
                if (result == null || result.ContainsError)
                {
                    return new NotFoundResult();
                }

                return new OkObjectResult(result.Content);
            }
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<IActionResult> DeleteClient(string id, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }
            
            try
            {
                var subject = User.GetSubject();
                var result = await _clientActions.Delete(subject, id, type);
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
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<IActionResult> SearchClients(SearchClientsRequest parameter, EndpointTypes type)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            try
            {
                var result = await _clientActions.Search(User.GetSubject(), parameter, type);
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
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<IActionResult> AddClient(ClientResponse parameter, EndpointTypes type)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            try
            {
                var result = await _clientActions.Add(User.GetSubject(), parameter, type);
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

                return new OkObjectResult(result.Content);
            }
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private async Task<IActionResult> UpdateClient(UpdateClientRequest parameter, EndpointTypes type)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }
            
            try
            {
                var result = await _clientActions.Update(User.GetSubject(), parameter, type);
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
            catch (ResourceManagerException ex)
            {
                return this.GetError(ex.Code, ex.Message, HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                return this.GetError(ex.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
