
namespace SimpleIdentityServer.Core.Factories
{
    public interface IEncryptedPasswordFactory
    {
        string Encrypt(string password);
    }
}
