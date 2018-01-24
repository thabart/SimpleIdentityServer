using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using SimpleIdentityServer.ResourceManager.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.EF.Repositories
{
    internal class AssetRepository : IAssetRepository
    {
        private readonly ResourceManagerDbContext _context;

        public AssetRepository(ResourceManagerDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AssetAggregate>> Search(SearchAssetsParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IQueryable<Asset> assets = _context.Assets.Include(a => a.Parent).Include(a => a.Children);
            if (parameter.HashLst != null && parameter.HashLst.Any())
            {
                assets = assets.Where(a => parameter.HashLst.Contains(a.Hash));
            }

            if (parameter.ExcludedHashLst != null && parameter.ExcludedHashLst.Any())
            {
                assets = assets.Where(a => !parameter.ExcludedHashLst.Contains(a.Hash));
            }

            switch (parameter.AssetLevelType)
            {
                case AssetLevelTypes.ROOT:
                    assets = assets.Where(a => a.Parent == null);
                    break;
            }

            if (parameter.IsDefaultWorkingDirectory != null)
            {
                assets = assets.Where(a => a.IsDefaultWorkingDirectory == parameter.IsDefaultWorkingDirectory.Value);
            }

            var result = await assets.ToListAsync().ConfigureAwait(false);
            return result.Select(a => GetAsset(a));
        }

        public async Task<AssetAggregate> Get(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentNullException(nameof(hash));
            }

            var asset = await _context.Assets.Include(a => a.Parent).FirstOrDefaultAsync(a => a.Hash == hash).ConfigureAwait(false);
            if (asset == null)
            {
                return null;
            }

            return GetAsset(asset);
        }

        public async Task<IEnumerable<AssetAggregate>> GetAllParents(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentNullException(nameof(hash));
            }

            var asset = await _context.Assets.Include(a => a.Parent).FirstOrDefaultAsync(a => a.Hash == hash).ConfigureAwait(false);
            if (asset == null)
            {
                return new List<AssetAggregate>();
            }

            var result = new List<AssetAggregate>();
            if (asset.Parent != null)
            {
                result.Add(GetAsset(asset.Parent));
                result.AddRange(await GetAllParents(asset.Parent.Hash));
            }

            return result;
        }

        public async Task<bool> Add(AssetAggregate asset)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = new Asset
                    {
                        CanRead = asset.CanRead,
                        CanWrite = asset.CanWrite,
                        CreateDateTime = asset.CreatedAt,
                        Hash = asset.Hash,
                        IsDefaultWorkingDirectory = false,
                        IsLocked = asset.IsLocked,
                        Name = asset.Name,
                        ResourceParentHash = asset.ResourceParentHash,
                        Path = asset.Path
                    };

                    _context.Assets.Add(record);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                }
            }

            return false;
        }

        private static AssetAggregate GetAsset(Asset asset)
        {
            return new AssetAggregate
            {
                Hash = asset.Hash,
                ResourceParentHash = asset.ResourceParentHash,
                CreatedAt = asset.CreateDateTime,
                Path = asset.Path,
                Name = asset.Name,
                CanRead = asset.CanRead,
                CanWrite = asset.CanWrite,
                IsLocked = asset.IsLocked,
                IsDefaultWorkingDirectory = asset.IsDefaultWorkingDirectory,
                Children = asset.Children == null ? new List<AssetAggregate>() : asset.Children.Select(a => GetAsset(a))
            };
        }
    }
}
