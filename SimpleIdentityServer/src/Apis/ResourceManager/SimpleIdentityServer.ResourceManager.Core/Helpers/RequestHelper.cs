using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Manager.Common.Requests;
using SimpleIdentityServer.Manager.Common.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.ResourceManager.Core.Helpers
{
    public interface IRequestHelper
    {
        void UpdateClientResponseTypes(ClientResponse client);
        void UpdateClientResponseTypes(UpdateClientRequest client);
    }

    internal sealed class RequestHelper : IRequestHelper
    {
        public void UpdateClientResponseTypes(ClientResponse client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (client.GrantTypes == null || !client.GrantTypes.Any())
            {
                return;
            }

            var responseTypes = new List<string>
            {
                "token",
                "id_token"
            };
            if (client.GrantTypes.Contains("authorization_code"))
            {
                responseTypes.Add("code");
            }

            client.ResponseTypes = responseTypes;
        }
        public void UpdateClientResponseTypes(UpdateClientRequest client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (client.GrantTypes == null || !client.GrantTypes.Any())
            {
                return;
            }

            var responseTypes = new List<ResponseType>
            {
                ResponseType.id_token,
                ResponseType.token
            };
            if (client.GrantTypes.Contains(GrantType.authorization_code))
            {
                responseTypes.Add(ResponseType.code);
            }

            client.ResponseTypes = responseTypes;
        }
    }
}
