using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Services
{
    public interface ISubjectBuilder
    {
        Task<string> BuildSubject();
    }
}
