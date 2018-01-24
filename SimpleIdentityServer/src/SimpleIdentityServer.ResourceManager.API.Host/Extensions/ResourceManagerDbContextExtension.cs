using SimpleIdentityServer.ResourceManager.EF;
using SimpleIdentityServer.ResourceManager.EF.Models;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace SimpleIdentityServer.ResourceManager.API.Host.Extensions
{
    internal static class ResourceManagerDbContextExtension
    {
        public static void EnsureSeedData(this ResourceManagerDbContext resourceManagerContext)
        {
            if (resourceManagerContext == null)
            {
                throw new ArgumentNullException(nameof(resourceManagerContext));
            }

            AddAssets(resourceManagerContext);
            resourceManagerContext.SaveChanges();
        }

        private static void AddAssets(ResourceManagerDbContext context)
        {
            if (!context.Assets.Any())
            {
                context.Assets.AddRange(new[]
                {
                   new Asset
                   {
                       Hash = GetHash("Root"),
                       Name = "Root",
                       Path = "Root",
                       IsDefaultWorkingDirectory = true
                   },
                   new Asset
                   {
                       Hash = GetHash("Second root"),
                       Name = "Second root",
                       Path = "Second root"
                   },
                   new Asset
                   {
                       Hash = GetHash("Root/Sub"),
                       ResourceParentHash = GetHash("Root"),
                       Name = "Sub",
                       Path = "Root/Sub"
                   },
                   new Asset
                   {
                       Hash = GetHash("Root/another directory"),
                       ResourceParentHash = GetHash("Root"),
                       Name = "another directory",
                       Path = "Root/another directory"
                   }
                });
            }
        }

        private static string GetHash(string path)
        {
            if (string.IsNullOrEmpty(path))
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
