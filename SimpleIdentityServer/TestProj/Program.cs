using System;
using System.Security.Cryptography;
using System.Text;
using SimpleIdentityServer.Core.Extensions;

namespace TestProj
{
    class Program
    {
        private static void Sign()
        {
            var toEncrypt = "coucou";
            byte[] b,
                bytes;
            string exported;
            using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
            {
                var parameters = rsaCryptoServiceProvider.ExportParameters(false);
                exported = rsaCryptoServiceProvider.ToXmlString(true);
            }

            using (var provider = new RSACryptoServiceProvider())
            {
                bytes = ASCIIEncoding.ASCII.GetBytes(toEncrypt);
                provider.FromXmlString(exported);
                b = provider.SignData(bytes, "SHA256");
            }

            using (var provider2 = new RSACryptoServiceProvider())
            {
                provider2.FromXmlString(exported);
                var result = provider2.VerifyData(bytes, "SHA256", b);
                Console.WriteLine(result);
            }

            using (var ec = new ECDiffieHellmanCng())
            {
                ec.ToXmlString(ECKeyXmlFormat.Rfc4050);
            }
        }

        static void Main(string[] args)
        {
            var encoded = "TXlCbG9nOk15QmxvZw==";
            var b = encoded.Base64Decode();
            Console.WriteLine(b);
        }
    }
}
