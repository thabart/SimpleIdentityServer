using System.Collections.Generic;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
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

        bool ValidateResponseType(List<ResponseType> responseTypes, Client client);
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
                return url;
            }

            return client.RedirectionUrls.FirstOrDefault(r => r == url);
        }

        public bool ValidateGrantType(GrantType grantType, Client client)
        {
            if (client == null)
            {
                return false;
            }

            return client.GrantTypes != null && client.GrantTypes.Contains(grantType);
        }

        public bool ValidateResponseType(List<ResponseType> responseTypes, Client client)
        {
            if (client == null || responseTypes == null)
            {
                return false;
            }

            return client.ResponseTypes != null && responseTypes.All(rt => client.ResponseTypes.Contains(rt));
        }
    }
}
