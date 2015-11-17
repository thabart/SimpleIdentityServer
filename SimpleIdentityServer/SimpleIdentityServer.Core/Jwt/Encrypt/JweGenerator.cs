using System.Security.Cryptography;
using System.Text;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.Core.Jwt.Encrypt
{
    public interface IJweGenerator
    {
        string GenerateJwe(
            string entry,
            string clientId);
    }

    public class JweGenerator : IJweGenerator
    {
        private IClientRepository _clientRepository;

        public JweGenerator(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public string GenerateJwe(
            string entry,
            string clientId)
        {
            var client = _clientRepository.GetClientById(clientId);

            using (var rsa = new RSACryptoServiceProvider())
            {
                var dataToEncrypt = ASCIIEncoding.ASCII.GetBytes("data to encrypt");
                var encryptedData = RsaEncrypt(dataToEncrypt, rsa.ExportParameters(false), false);
            }

            // Content encryption key.
            var cek = "";

            var macKey = "";
            var encKey = "";

            return string.Empty;
        }

        private static byte[] RsaEncrypt(byte[] dataToEncrypt, RSAParameters rsaKeyInfo, bool DoOAEPPadding)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaKeyInfo);
                return rsa.Encrypt(dataToEncrypt, DoOAEPPadding);
            }
        }
    }
}
