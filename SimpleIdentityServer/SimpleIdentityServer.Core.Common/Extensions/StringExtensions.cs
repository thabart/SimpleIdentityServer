using System;
using System.Text;

namespace SimpleIdentityServer.Core.Common.Extensions
{
    public static class StringExtensions
    {
        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            return Encoding.UTF8.GetString(base64EncodedData.Base64DecodeBytes());
        }

        public static byte[] Base64DecodeBytes(this string base64EncodedData)
        {
            base64EncodedData = base64EncodedData.Trim();
            base64EncodedData = base64EncodedData.Replace("-", "+");
            base64EncodedData = base64EncodedData.Replace("_", "/");
            base64EncodedData = base64EncodedData.Replace(" ", "+");
            switch (base64EncodedData.Length % 4)
            {
                case 0:
                    break;
                case 2:
                    base64EncodedData += "==";
                    break;
                case 3:
                    base64EncodedData += "=";
                    break;
            }

            return Convert.FromBase64String(base64EncodedData);
        }
    }
}
