using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.ResourceManager.API.Host.DTOs;
using SimpleIdentityServer.ResourceManager.API.Host.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ElFinterController)]
    public class ElFinderController : Controller
    {
        private readonly IAssetRepository _assetRepository;

        public ElFinderController(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] JObject json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            var deserializedParameter = ElFinderParameter.Deserialize(json); // 1. Extract the parameter.
            if (deserializedParameter.ErrorResponse != null)
            {
                return new OkObjectResult(deserializedParameter.ErrorResponse.GetJson());
            }
                       
            switch(deserializedParameter.ElFinderParameter.Command)
            {
                case ElFinderCommands.Open:
                    return new OkObjectResult(await ExecuteOpen(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Parents:
                    return new OkObjectResult(await ExecuteParents(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Mkdir:
                    return new OkObjectResult(await ExecuteMkdir(deserializedParameter.ElFinderParameter));
            }

            return new OkResult();
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#open
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteOpen(ElFinderParameter elFinderParameter)
        {
            AssetAggregate rootAsset = null;  // 1. Search the assets.
            IEnumerable<AssetAggregate> targetAsset = null;
            if (!string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                var searchTargetParameter = new SearchAssetsParameter
                {
                    AssetLevelType = AssetLevelTypes.ALL,
                    HashLst = new[] { elFinderParameter.Target }
                };
                targetAsset = await _assetRepository.Search(searchTargetParameter);
                if (targetAsset == null || !targetAsset.Any())
                {
                    return new ErrorResponse(Constants.ElFinderErrors.ErrOpen).GetJson();
                }

                rootAsset = targetAsset.First();
            }

            var searchRootParameter = new SearchAssetsParameter
            {
                AssetLevelType = AssetLevelTypes.ROOT,
                ExcludedHashLst = string.IsNullOrWhiteSpace(elFinderParameter.Target) ? null : new[] { elFinderParameter.Target }
            };
            var rootAssets = await _assetRepository.Search(searchRootParameter);
            if (rootAsset == null)
            {
                rootAsset = rootAssets.First(f => f.IsDefaultWorkingDirectory);
            }

            var result = new JObject(); // 3. Return the result.
            if (elFinderParameter.Init)
            {
                result.Add(Constants.ElFinderResponseNames.Api, "2.1");
            }

            var rootJson = GetFile(rootAsset);
            var files = new JArray();
            foreach (var record in rootAssets)
            {
                files.Add(GetFile(record));
            }

            files.Add(GetFile(rootAsset));
            foreach (var record in rootAsset.Children)
            {
                files.Add(GetFile(record));
            }

            var opts = new JObject();
            opts.Add(Constants.ElFinderOptionNames.Path, rootAsset.Path);
            opts.Add(Constants.ElFinderOptionNames.Disabled, new JArray(new[] { "chmod" }));
            opts.Add(Constants.ElFinderOptionNames.Separator, "/");
            result.Add(Constants.ElFinderResponseNames.UplMaxSize, "0");
            result.Add(Constants.ElFinderResponseNames.Cwd, rootJson);
            result.Add(Constants.ElFinderResponseNames.Files, files);
            result.Add(Constants.ElFinderResponseNames.Options, opts);
            return result;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#parents
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteParents(ElFinderParameter elFinderParameter)
        {
            var asset = await _assetRepository.Get(elFinderParameter.Target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrOpen).GetJson();
            }

            var parents = await _assetRepository.GetAllParents(elFinderParameter.Target);
            var files = new JArray();
            foreach (var parent in parents)
            {
                files.Add(GetFile(parent));
            }
            
            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.Tree, files);
            return result;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#mkdir
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteMkdir(ElFinderParameter elFinderParameter)
        {
            var asset = await _assetRepository.Get(elFinderParameter.Target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrOpen).GetJson();
            }

            var recordPath = asset.Path + "/" + elFinderParameter.Name;
            var record = new AssetAggregate
            {
                CanRead = true,
                CanWrite = true,
                IsLocked = false,
                Name = elFinderParameter.Name,
                CreatedAt = DateTime.UtcNow,
                ResourceParentHash = asset.Hash,
                Path = recordPath,
                Hash = HashHelper.GetHash(recordPath)
            };

            if (!(await _assetRepository.Add(record)))
            {
                return new ErrorResponse(Constants.Errors.ErrInsertAsset).GetJson();
            }

            var files = new JArray(GetFile(record));
            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.Added, files);
            return result;
        }

        private static JObject GetFile(AssetAggregate asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            return AssetResponse.CreateDirectory(asset.Name, asset.Hash, Constants.VolumeId + "_", asset.Children.Any(), asset.ResourceParentHash,
                new AssetSecurity(asset.CanRead, asset.CanWrite, asset.IsLocked)).GetJson();
        }
    }
}
