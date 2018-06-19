using System.Threading.Tasks;

namespace SimpleIdentityServer.Store
{
    public interface IConfirmationCodeStore
    {
        Task<ConfirmationCode> Get(string code);
        Task<bool> Add(ConfirmationCode confirmationCode);
        Task<bool> Remove(string code);
    }
}
