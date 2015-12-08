using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
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
                if (!Request.Headers.Contains("Authorization"))
                {
                    throw new AuthorizationException(ErrorCodes.InvalidToken, string.Empty);
                }

                var authenticationHeader = Request.Headers.GetValues("Authorization").First();
                var authorization = AuthenticationHeaderValue.Parse(authenticationHeader);

                var scheme = authorization.Scheme;
                if (string.Compare(scheme, "Bearer", StringComparison.InvariantCultureIgnoreCase) != 0)
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
            catch (AuthorizationException)
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