using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Repositories
{
    public interface IEndpointRepository
    {
        Task<EndpointAggregate> Get(string url);
        Task<bool> Add(IEnumerable<EndpointAggregate> idProviders);
        Task<IEnumerable<EndpointAggregate>> GetAll();
        Task<IEnumerable<EndpointAggregate>> Search(SearchEndpointsParameter parameter);
    }
}
