using SimpleIdentityServer.ResourceManager.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.Core.Repositories
{
    public interface IProfileRepository
    {
        Task<ProfileAggregate> Get(string subject);
        Task<bool> Add(IEnumerable<ProfileAggregate> profiles);
        Task<bool> Delete(IEnumerable<string> subjects);
        Task<bool> Update(IEnumerable<ProfileAggregate> profiles);
    }
}
