namespace SimpleIdentityServer.Core.Jwt.Encrypt.Algorithms
{
    public interface IAlgorithm
    {
        byte[] Encrypt(
            byte[] toBeEncrypted,
            JsonWebKey jsonWebKey);

        byte[] Decrypt(
            byte[] toBeDecrypted,
            JsonWebKey jsonWebKey);
    }
}
