namespace SimpleIdentityServer.Core.Jwt.Encrypt.Encryption
{
    public interface IEncryption
    {
        AesEncryptionResult Encrypt(
            string toEncrypt,
            JweAlg alg,
            JweProtectedHeader protectedHeader,
            JsonWebKey jsonWebKey);

        AesEncryptionResult EncryptWithSymmetricPassword(
            string toEncrypt,
            JweAlg alg,
            JweProtectedHeader protectedHeader,
            JsonWebKey jsonWebKey,
            string password);

        string Decrypt(
            string toDecrypt,
            JweAlg alg,
            JsonWebKey jsonWebKey);

        string DecryptWithSymmetricPassword(
            string toDecrypt,
            JweAlg alg,
            JsonWebKey jsonWebKey,
            string password);
    }
}
