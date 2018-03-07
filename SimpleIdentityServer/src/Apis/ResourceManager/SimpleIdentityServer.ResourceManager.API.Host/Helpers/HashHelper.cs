using SimpleIdentityServer.ResourceManager.API.Host.Extensions;
using System;
using System.Security.Cryptography;

namespace SimpleIdentityServer.ResourceManager.API.Host.Helpers
{
    internal static class HashHelper
    {
        public static string GetHash(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }


            using (var mySHA256 = SHA256.Create())
            {
                var hash = mySHA256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(path));
                var h = hash.ToHexString();
                return string.Format("{0}_{1}", Constants.VolumeId, h);
            }
        }
    }
}
