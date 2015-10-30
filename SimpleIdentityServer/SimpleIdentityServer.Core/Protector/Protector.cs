using System;
using System.IdentityModel;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Script.Serialization;

namespace SimpleIdentityServer.Core.Protector
{
    public interface IProtector
    {
        string Encrypt(object obj);

        T Decrypt<T>(string encryptedString);
    }

    public class Protector : IProtector
    {
        private readonly ICertificateStore _certificateStore;

        private readonly ICompressor _compressor;

        private readonly CookieTransform _encrypt;

        private readonly CookieTransform _sign;

        public Protector(
            ICertificateStore certificateStore,
            ICompressor compressor)
        {
            _certificateStore = certificateStore;
            _compressor = compressor;

            var certificate = CheckParameters();
            _encrypt = new RsaEncryptionCookieTransform(certificate);
            _sign = new RsaSignatureCookieTransform(certificate);
        }

        public string Encrypt(object obj)
        {
            var json = new JavaScriptSerializer().Serialize(obj);
            var compressedJson = _compressor.Compress(json);
            var bytesToEncrypt = ASCIIEncoding.ASCII.GetBytes(compressedJson);
            var encryptedBytes = _encrypt.Encode(bytesToEncrypt);
            var signedBytes = _sign.Encode(encryptedBytes);
            return Convert.ToBase64String(signedBytes);
        }

        public T Decrypt<T>(string encryptedString)
        {
            var bytesToDecrypt = Convert.FromBase64String(encryptedString);
            var validated = _sign.Decode(bytesToDecrypt);
            var plainBytes = _encrypt.Decode(validated);
            var encoding = new ASCIIEncoding();
            var decryptedJson = encoding.GetString(plainBytes);
            var uncompressedJson = _compressor.Decompress(decryptedJson);
            return new JavaScriptSerializer().Deserialize<T>(uncompressedJson);
        }

        private X509Certificate2 CheckParameters()
        {
            if (_certificateStore == null || _certificateStore.Get() == null)
            {
                throw new NullReferenceException("there's no certificate store");
            }

            return _certificateStore.Get();
        }
    }
}
