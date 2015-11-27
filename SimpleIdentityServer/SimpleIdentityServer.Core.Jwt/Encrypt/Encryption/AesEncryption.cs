namespace SimpleIdentityServer.Core.Jwt.Encrypt.Encryption
{
    public class AesEncryption : IEncryption
    {
        public AesEncryptionResult Encrypt(
            string toEncrypt,
            JweAlg alg, 
            JweProtectedHeader protectedHeader,
            JsonWebKey jsonWebKey)
        {
            return null;
        }

        public AesEncryptionResult EncryptWithSymmetricPassword(
            string toEncrypt, 
            JweAlg alg, 
            JweProtectedHeader protectedHeader, 
            JsonWebKey jsonWebKey,
            string password)
        {
            return null;
        }
        
        public string Decrypt(
            string toDecrypt, 
            JweAlg alg, 
            JsonWebKey jsonWebKey)
        {
            return string.Empty;
        }

        public string DecryptWithSymmetricPassword(
            string toDecrypt, 
            JweAlg alg, 
            JsonWebKey jsonWebKey,
            string password)
        {
            return string.Empty;
        }
    }
}
