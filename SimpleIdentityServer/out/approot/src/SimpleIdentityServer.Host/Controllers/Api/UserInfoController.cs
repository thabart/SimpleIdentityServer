using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using SimpleIdentityServer.Core.Api.UserInfo;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using SimpleIdentityServer.Host;
using Microsoft.Extensions.Primitives;
using SimpleIdentityServer.Host.Extensions;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    [Route(Constants.EndPoints.UserInfo)]
    public class UserInfoController : Controller
    {
        private readonly IUserInfoActions _userInfoActions;

        public UserInfoController(IUserInfoActions userInfoActions)
        {
            _userInfoActions = userInfoActions;
        }

        #region Public methods

        [HttpGet]
        public ActionResult Get()
        {
            return ProcessRequest();
        }

        [HttpPost]
        public ActionResult Post()
        {
            return ProcessRequest();
        }
        
        #endregion
        
        #region Private methods

        private ActionResult ProcessRequest()
        {
            try
            {
                var accessToken = TryToGetTheAccessToken();
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new AuthorizationException(ErrorCodes.InvalidToken, string.Empty);
                }

                var result = _userInfoActions.GetUserInformation(accessToken);
                return result.Content;
            }
            catch (AuthorizationException)
            {
                return new HttpUnauthorizedResult();
            }
            catch (Exception ex)
            {
                return new HttpOkObjectResult(ex.StackTrace);
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
            StringValues values;
            if (!Request.Headers.TryGetValue(authorizationName, out values)) {
                return string.Empty;
            }

            var authenticationHeader = values.First();
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
            StringValues values;
            if (Request.Headers == null 
                || !Request.Headers.TryGetValue(contentTypeName, out values))
            {
                return emptyResult;
            }

            var contentTypeHeader = values.First();
            if (string.Compare(contentTypeHeader, contentTypeValue) !=  0)
            {
                return emptyResult;
            }

            var content = Request.ReadAsStringAsync().Result;
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
            var query = Request.Query;
            var record = query.FirstOrDefault(q => q.Key == accessTokenName);
            if (record.Equals(default(KeyValuePair<string, StringValues>))) {
                return string.Empty;
            } else {
                return record.Value.First();
            }
        }
        
        #endregion
    }
}