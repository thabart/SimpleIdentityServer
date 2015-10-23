using System.Security.Cryptography.X509Certificates;

namespace SimpleIdentityServer.Core.Protector
{
    public interface ICertificateStore
    {
        X509Certificate2 Get();
    }
}
