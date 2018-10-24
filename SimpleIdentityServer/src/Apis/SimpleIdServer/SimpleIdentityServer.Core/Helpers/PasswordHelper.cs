using System;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Core.Helpers
{
    public static class PasswordHelper
    {
        public static string ComputeHash(string entry)
        {
            using (var sha256 = SHA256.Create())
            {
                var entryBytes = Encoding.UTF8.GetBytes(entry);
                var hash = sha256.ComputeHash(entryBytes);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
