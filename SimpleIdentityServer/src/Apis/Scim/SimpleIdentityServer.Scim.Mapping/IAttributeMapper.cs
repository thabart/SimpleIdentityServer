using SimpleIdentityServer.Scim.Common.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Scim.Mapping
{
    public interface IAttributeMapper
    {
        Task Map(Representation representation, string schemaId);
    }
}