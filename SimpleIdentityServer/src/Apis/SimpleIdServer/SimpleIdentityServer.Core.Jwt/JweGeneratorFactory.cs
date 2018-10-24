using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Core.Jwt.Encrypt.Encryption;

namespace SimpleIdentityServer.Core.Jwt
{
    public class JweGeneratorFactory
    {
        public IJweGenerator BuildJweGenerator()
        {
            return new JweGenerator(new JweHelper(new AesEncryptionHelper()));
        }
    }
}