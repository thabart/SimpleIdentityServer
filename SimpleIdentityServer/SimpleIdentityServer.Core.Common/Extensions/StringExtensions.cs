using System;
using System.Text;

namespace SimpleIdentityServer.Core.Common.Extensions
{
    public static class StringExtensions
    {
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return plainTextBytes.Base64EncodeBytes();
        }

        public static string Base64EncodeBytes(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes).Split('=')[0].Replace('+', '-').Replace('/', '_');
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            return Encoding.UTF8.GetString(base64EncodedData.Base64DecodeBytes());
        }

        public static byte[] Base64DecodeBytes(this string base64EncodedData)
        {
            try
            {

                var s = base64EncodedData.Replace('-', '+').Replace('_', '/');
                switch (s.Length%4)
                {
                    case 0:
                        return Convert.FromBase64String(s);
                    case 2:
                        s += "==";
                        goto case 0;
                    case 3:
                        s += "=";
                        goto case 0;
                    default:
                        throw new Exception("Illegal base64url string!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("error : " + ex.Message);
            }
        }
    }
}
