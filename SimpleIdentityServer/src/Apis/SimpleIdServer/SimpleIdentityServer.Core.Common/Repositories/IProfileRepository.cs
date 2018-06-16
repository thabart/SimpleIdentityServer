using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Common.Repositories
{
    public interface IProfileRepository
    {
        Task<ResourceOwnerProfile> Get(string subject);
        Task<bool> Add(IEnumerable<ResourceOwnerProfile> profiles);
        Task<IEnumerable<ResourceOwnerProfile>> Search(SearchProfileParameter parameter);
        Task<bool> Remove(IEnumerable<string> subjects);
    }
}