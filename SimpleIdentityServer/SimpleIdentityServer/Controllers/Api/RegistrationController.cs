using System;
using SimpleIdentityServer.Api.DTOs.Response;
using SimpleIdentityServer.Api.Extensions;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Results;

using Swashbuckle.Swagger.Annotations;

using System.Web.Http;

namespace SimpleIdentityServer.Api.Controllers.Api
{
    public class RegistrationController : ApiController
    {
        private readonly IRegistrationActions _registerActions;

        public RegistrationController(IRegistrationActions registerActions)
        {
            _registerActions = registerActions;
        }

        [SwaggerOperation("RegisterOperation")]
        public RegistrationResponse Post(ClientResponse client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            
            return _registerActions.PostRegistration(client.ToParameter());
        }
    }
}