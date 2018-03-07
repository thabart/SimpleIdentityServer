using System.Text;

namespace SimpleIdentityServer.ResourceManager.API.Host.Extensions
{
    internal static class StringExtensions
    {
        public static string ToHexString(this byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}
