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
    }

    public class ClientValidator : IClientValidator
    {
        private readonly IClientRepository _clientRepository;

        public ClientValidator(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public Client ValidateClientExist(string clientId)
        {
            var client = _clientRepository.GetClientById(clientId);
            if (client == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidClient,
                    string.Format(ErrorDescriptions.ClientIsNotValid, "client_id"));
            }

            return client;
        }
        
        public string ValidateRedirectionUrl(string url, Client client)
        {
            var redirectionUrl = client.RedirectionUrls.FirstOrDefault(r => r == url);
            if (redirectionUrl == null)
            {
                throw new IdentityServerException(
                    ErrorCodes.InvalidRequestUriCode,
                    string.Format(ErrorDescriptions.RedirectUrlIsNotValid, redirectionUrl));
            }

            return redirectionUrl;
        }
    }
}
