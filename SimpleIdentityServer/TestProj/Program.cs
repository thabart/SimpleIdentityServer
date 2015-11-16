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
            var today = DateTime.UtcNow.AddDays(2).ToString();
            var encoded =
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJub25lIiwia2lkIjpudWxsfQ==.eyJpc3MiOiJodHRwOi8vbG9jYWxob3N0L2lkZW50aXR5IiwiYXVkIjpbIk15QmxvZyJdLCJleHAiOjE0NTA2ODYzMDcsImlhdCI6MTQ0NzY4NjMwNywic3ViIjoiYWRtaW5pc3RyYXRvckBob3RtYWlsLmJlIiwiYWNyIjoib3BlbmlkLnBhcGUuYXV0aF9sZXZlbC5ucy5wYXNzd29yZD0xIiwiYW1yIjoicGFzc3dvcmQiLCJhenAiOiJNeUJsb2cifQ==.";
            var arr = encoded.Split('.');
            var header = arr[0].Base64Decode();
            var payload = arr[1].Base64Decode();
            Console.WriteLine(header);
            Console.WriteLine(payload);
            Console.ReadLine();
        }
    }
}
