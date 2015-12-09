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
using System.Collections.Generic;

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
                var accessToken = TryToGetTheAccessToken();
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

        private string TryToGetTheAccessToken()
        {
            var accessToken = GetAccessTokenFromAuthorizationHeader();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return accessToken;
            }

            accessToken = GetAccessTokenFromBodyParameter();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                return accessToken;
            }

            return GetAccessTokenFromQueryString();
        }

        /// <summary>
        /// Get an access token from the authorization header.
        /// </summary>
        /// <returns></returns>
        private string GetAccessTokenFromAuthorizationHeader()
        {
            const string authorizationName = "Authorization";
            if (!Request.Headers.Contains(authorizationName))
            {
                return string.Empty;
            }

            var authenticationHeader = Request.Headers.GetValues(authorizationName).First();
            var authorization = AuthenticationHeaderValue.Parse(authenticationHeader);

            var scheme = authorization.Scheme;
            if (string.Compare(scheme, "Bearer", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return string.Empty;
            }

            return authorization.Parameter;
        }

        /// <summary>
        /// Get an access token from the body parameter.
        /// </summary>
        /// <returns></returns>
        private string GetAccessTokenFromBodyParameter()
        {
            const string contentTypeName = "Content-Type";
            const string contentTypeValue = "application/x-www-form-urlencoded";
            string accessTokenName = Core.Constants.StandardAuthorizationResponseNames.AccessTokenName;
            var emptyResult = string.Empty;
            if (Request.Content == null 
                || !Request.Content.Headers.Contains(contentTypeName))
            {
                return emptyResult;
            }

            var contentTypeHeader = Request.Content.Headers.GetValues(contentTypeName).First();
            if (string.Compare(contentTypeHeader, contentTypeValue) !=  0)
            {
                return emptyResult;
            }

            var content = Request.Content.ReadAsStringAsync().Result;
            var queryString = HttpUtility.ParseQueryString(content);
            if (!queryString.AllKeys.Contains(accessTokenName))
            {
                return emptyResult;
            }

            return queryString.GetValues(accessTokenName).First();
        }

        /// <summary>
        /// Get an access token from the query string
        /// </summary>
        /// <returns></returns>
        private string GetAccessTokenFromQueryString()
        {
            string accessTokenName = Core.Constants.StandardAuthorizationResponseNames.AccessTokenName;
            var queryString = Request.GetQueryNameValuePairs();
            var record = queryString.FirstOrDefault(q => q.Key == accessTokenName);
            return record.Equals(default(KeyValuePair<string, string>)) ? string.Empty : record.Value;
        }
    }
}