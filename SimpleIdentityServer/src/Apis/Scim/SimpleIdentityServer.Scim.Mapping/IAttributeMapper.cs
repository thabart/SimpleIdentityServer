using SimpleIdentityServer.Scim.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping
{
    public interface IAttributeMapper
    {
        Task<string> Map(Representation representation, string attributeId);
    }
}