using System;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;

using System.Linq;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IClientValidator
    {
        Client ValidateClientExist(string clientId);

        string ValidateRedirectionUrl(string url, Client client);

        bool ValidateGrantType(GrantType grantType, Client client);

        bool ValidateGrantTypes(IList<GrantType> grantTypes, Client client);

        bool ValidateResponseType(ResponseType responseType, Client client);

        bool ValidateResponseTypes(IList<ResponseType> responseType, Client client);
    }

    public class ClientValidator : IClientValidator
    {
        private readonly IClientRepository _clientRepository;

        public ClientValidator(
            IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public Client ValidateClientExist(string clientId)
        {
            return _clientRepository.GetClientById(clientId);
        }
        
        public string ValidateRedirectionUrl(string url, Client client)
        {
            if (client.RedirectionUrls == null || !client.RedirectionUrls.Any())
            {
                return null;
            }

            return client.RedirectionUrls.FirstOrDefault(r => r == url);
        }

        public bool ValidateGrantType(GrantType grantType, Client client)
        {
            if (client == null)
            {
                return false;
            }

            SetDefaultClientGrantType(client);
            return client.GrantTypes != null && client.GrantTypes.Contains(grantType);
        }

        public bool ValidateGrantTypes(IList<GrantType> grantTypes, Client client)
        {
            if (client == null)
            {
                return false;
            }

            SetDefaultClientGrantType(client);
            return client.GrantTypes != null && grantTypes.All(gt => client.GrantTypes.Contains(gt));
        }

        public bool ValidateResponseType(ResponseType responseType, Client client)
        {
            if (client == null)
            {
                return false;
            }

            SetDefaultClientResponseType(client);
            return client.ResponseTypes != null && client.ResponseTypes.Contains(responseType);
        }

        public bool ValidateResponseTypes(IList<ResponseType> responseTypes, Client client)
        {
            if (client == null)
            {
                return false;
            }

            SetDefaultClientResponseType(client);
            return client.ResponseTypes != null && responseTypes.All(rt => client.ResponseTypes.Contains(rt));
        }

        private static void SetDefaultClientGrantType(Client client)
        {
            if (client.GrantTypes == null || !client.GrantTypes.Any())
            {
                client.GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code
                };
            }
        }

        private static void SetDefaultClientResponseType(Client client)
        {
            if (client.ResponseTypes == null || !client.ResponseTypes.Any())
            {
                client.ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                };
            }
        }
    }
}
