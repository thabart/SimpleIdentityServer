using System.IO;
using System.Security.Cryptography.X509Certificates;
using SimpleIdentityServer.Core.Protector;

namespace SimpleIdentityServer.Api.Configuration
{
    public class CertificateStore : ICertificateStore
    {
        public X509Certificate2 Get()
        {
            var assembly = typeof (CertificateStore).Assembly;
            using (var stream = assembly.GetManifestResourceStream("SimpleIdentityServer.Api.SimpleIdentityServer.pfx"))
            {
                return new X509Certificate2(ReadStream(stream), "loki", X509KeyStorageFlags.Exportable);
            }
        }

        private static byte[] ReadStream(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}