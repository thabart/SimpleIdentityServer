using System;
using SimpleIdentityServer.Core.Api.UserInfo;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class UserInfoController : ApiController
    {
        private readonly IUserInfoActions _userInfoActions;

        public UserInfoController(IUserInfoActions userInfoActions)
        {
            _userInfoActions = userInfoActions;
        }

        public HttpResponseMessage Get()
        {
            return ProcessRequest();
        }

        public HttpResponseMessage Post()
        {
            return ProcessRequest();
        }

        private HttpResponseMessage ProcessRequest()
        {
            try
            {
                // Different way to pass the access_token ?
                var authorization = Request.Headers.Authorization;
                if (authorization == null)
                {
                    throw new AuthorizationException(ErrorCodes.InvalidToken, string.Empty);
                }

                var scheme = authorization.Scheme;
                if (scheme != "Bearer")
                {
                    throw new AuthorizationException(ErrorCodes.InvalidToken, string.Empty);
                }

                var accessToken = authorization.Parameter;
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new AuthorizationException(ErrorCodes.InvalidToken, string.Empty);
                }

                var result = _userInfoActions.GetUserInformation(accessToken);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (AuthorizationException authorizationException)
            {
                var response = Request.CreateResponse(HttpStatusCode.Unauthorized);
                // TODO : add the code & description into the WWW-Authenticate response header.
                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, ex.StackTrace);
            }
        }
    }
}