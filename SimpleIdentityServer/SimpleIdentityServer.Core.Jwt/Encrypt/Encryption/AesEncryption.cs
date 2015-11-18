namespace SimpleIdentityServer.Core.Jwt.Encrypt.Encryption
{
    public class AesEncryption : IEncryption
    {
        public AesEncryptionResult Encrypt(string toEncrypt, JweAlg alg, JweProtectedHeader protectedHeader, JsonWebKey jsonWebKey)
        {
            return null;
        }
    }
}
