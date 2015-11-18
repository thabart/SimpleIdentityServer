namespace SimpleIdentityServer.Core.Jwt.Encrypt.Encryption
{
    public class AesEncryptionResult
    {
        public byte[] Iv { get; set; }

        public byte[] CipherText { get; set; }

        public byte[] EncryptedContentEncryptionKey { get; set; }

        public byte[] AuthenticationTag { get; set; }
    }
}
