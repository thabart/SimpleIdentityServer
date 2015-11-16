using System.Web.Http;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class UserInfoController : ApiController
    {
        public void Get()
        {
            
        }

        public void Post()
        {
            
        }

        private void ProcessRequest()
        {
            var authorization = Request.Headers.Authorization;
            if (authorization == null)
            {
                // TODO throw the appropriate exception.
            }

            var scheme = authorization.Scheme;
            if (scheme != "Bearer")
            {
                // TODO throw the appropriate exception
            }

            var accessToken = authorization.Parameter;
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                // TODO throw the appropriate exception
            }

            // Check the token
        }
    }
}