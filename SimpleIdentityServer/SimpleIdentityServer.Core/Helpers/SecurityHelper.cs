using System;
using System.Security.Cryptography;
using System.Text;

namespace SimpleIdentityServer.Core.Helpers
{
    public interface ISecurityHelper
    {
        string ComputeHash(string entry);
    }

    public class SecurityHelper : ISecurityHelper
    {
        public string ComputeHash(string entry)
        {
            using (var sha256 = SHA256Managed.Create())
            {
                var entryBytes = Encoding.UTF8.GetBytes(entry);
                var hash = sha256.ComputeHash(entryBytes);
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }
    }
}
