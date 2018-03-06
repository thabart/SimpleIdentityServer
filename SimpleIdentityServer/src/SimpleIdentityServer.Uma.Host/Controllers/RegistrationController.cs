using Microsoft.AspNetCore.Mvc;
using SimpleIdentityServer.Core.Api.Registration;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Uma.Host.DTOs.Responses;
using SimpleIdentityServer.Uma.Host.Extensions;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.Host.Controllers
{
    [Route(Constants.RouteValues.Registration)]
    public class RegistrationController : Controller
    {
        private readonly IRegistrationActions _registerActions;

        public RegistrationController(IRegistrationActions registerActions)
        {
            _registerActions = registerActions;
        }

        [HttpPost]
        public async Task<ClientRegistrationResponse> Post([FromBody] ClientResponse client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            return await _registerActions.PostRegistration(client.ToParameter());
        }
    }
}
