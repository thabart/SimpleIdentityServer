using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using SimpleIdentityServer.ResourceManager.Core.Api.Scopes;
using SimpleIdentityServer.ResourceManager.Core.Exceptions;
using SimpleIdentityServer.ResourceManager.Core.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ScopesController)]
    public class ScopesController : Controller
    {
        private readonly IScopeActions _scopeActions;

        public ScopesController(IScopeActions scopeActions)
        {
            _scopeActions = scopeActions;
        }
        
        [Authorize("connected")]
        [HttpGet("openid/{id}")]
        public Task<IActionResult> GetOpenIdScope(string id)
        {
            return GetScope(id, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpDelete("openid/{id}")]
        public Task<IActionResult> DeleteOpenIdScope(string id)
        {
            return DeleteScope(id, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpPost("openid")]
        public Task<IActionResult> AddOpenIdScope([FromBody] ScopeResponse scopeResponse)
        {
            return AddScope(scopeResponse, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpPut("openid")]
        public Task<IActionResult> UpdateOpenIdScope([FromBody] ScopeResponse scopeResponse)
        {
            return UpdateScope(scopeResponse, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpPost("openid/.search")]
        public Task<IActionResult> SearchOpenIdScopes([FromBody] SearchScopesRequest request)
        {
            return SearchScopes(request, EndpointTypes.OPENID);
        }

        [Authorize("connected")]
        [HttpGet("auth/{id}")]
        public Task<IActionResult> GetAuthScope(string id)
        {
            return GetScope(id, EndpointTypes.AUTH);
        }

        [Authorize("connected")]
        [HttpDelete("auth/{id}")]
        public Task<IActionResult> DeleteAuthScope(string id)
        {
            return DeleteScope(id, EndpointTypes.AUTH);
        }

        [Authorize("connected")]
        [HttpPost("auth")]
        public Task<IActionResult> AddAuthScope([FromBody] ScopeResponse scopeResponse)
        {
            return AddScope(scopeResponse, EndpointTypes.AUTH);
        }

        [Authorize("connected")]
        [HttpPut("auth")]
        public Task<IActionResult> UpdateAuthScope([FromBody] ScopeResponse scopeResponse)
        {
            return UpdateScope(scopeResponse, EndpointTypes.AUTH);
        }

        [Authorize("connected")]
        [HttpPost("auth/.search")]
        public Task<IActionResult> SearchAuthScopes([FromBody] SearchScopesRequest request)
        {
            return SearchScopes(request, EndpointTypes.AUTH);
        }

        public async Task<IActionResult> GetScope(string id, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                var subject = User.GetSubject();
                var result = await _scopeActions.Get(subject, id, type);
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

        private async Task<IActionResult> DeleteScope(string id, EndpointTypes type)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            try
            {
                var subject = User.GetSubject();
                var result = await _scopeActions.Delete(subject, id, type);
                if (result.ContainsError)
                {
                    var error = result.Error;
                    if (error == null)
                    {
                        error = new ErrorResponse
                        {
                            Code = Constants.ErrorCodes.InternalError,
                            Message = Constants.Errors.ErrDeleteScope
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

        private async Task<IActionResult> SearchScopes(SearchScopesRequest parameter, EndpointTypes type)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            try
            {
                var result = await _scopeActions.Search(User.GetSubject(), parameter, type);
                if (result.ContainsError)
                {
                    var error = result.Error;
                    if (error == null)
                    {
                        error = new ErrorResponse
                        {
                            Code = Constants.ErrorCodes.InternalError,
                            Message = Constants.Errors.ErrSearchScope
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

        private async Task<IActionResult> AddScope(ScopeResponse parameter, EndpointTypes type)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            try
            {
                var result = await _scopeActions.Add(User.GetSubject(), parameter, type);
                if (result.ContainsError)
                {
                    var error = result.Error;
                    if (error == null)
                    {
                        error = new ErrorResponse
                        {
                            Code = Constants.ErrorCodes.InternalError,
                            Message = Constants.Errors.ErrInsertScope
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

        private async Task<IActionResult> UpdateScope(ScopeResponse parameter, EndpointTypes type)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            try
            {
                var result = await _scopeActions.Update(User.GetSubject(), parameter, type);
                if (result.ContainsError)
                {
                    var error = result.Error;
                    if (error == null)
                    {
                        error = new ErrorResponse
                        {
                            Code = Constants.ErrorCodes.InternalError,
                            Message = Constants.Errors.ErrUpdateScope
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
