namespace SimpleIdentityServer.Core.Jwt.Encrypt.Encryption
{
    public interface IEncryption
    {
        AesEncryptionResult Encrypt(
            string toEncrypt,
            JweAlg alg,
            JweProtectedHeader protectedHeader,
            JsonWebKey jsonWebKey);
    }
}
