
namespace SimpleIdentityServer.Core.Services
{
    public interface IPasswordService
    {
        string Encrypt(string password);
    }
}
