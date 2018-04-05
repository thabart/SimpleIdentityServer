using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Results;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Manager.Core.Api.Clients.Actions
{
    public interface ISearchClientsAction
    {
        Task<SearchClientResult> Execute(SearchClientParameter parameter);
    }

    internal sealed class SearchClientsAction : ISearchClientsAction
    {
        private readonly IClientRepository _clientRepository;

        public SearchClientsAction(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public Task<SearchClientResult> Execute(SearchClientParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return _clientRepository.Search(parameter);
        }
    }
}
