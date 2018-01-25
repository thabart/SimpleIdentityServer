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
                case ElFinderCommands.Rm:
                    return new OkObjectResult(await ExecuteRm(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Rename:
                    return new OkObjectResult(await ExecuteRename(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Mkfile:
                    return new OkObjectResult(await ExecuteMkfile(deserializedParameter.ElFinderParameter));
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
            if (!elFinderParameter.Init && string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }
            
            var assets = new List<AssetAggregate>();
            AssetAggregate rootDirectory = null;
            if (elFinderParameter.Init)
            {
                if (string.IsNullOrWhiteSpace(elFinderParameter.Target)) // Returns the default root directory of the default volume.
                {
                    var searchRootParameter = new SearchAssetsParameter
                    {
                        AssetLevelType = AssetLevelTypes.ROOT,
                        IsDefaultWorkingDirectory = true
                    };
                    rootDirectory = (await _assetRepository.Search(searchRootParameter)).First();
                }
            }

            if (!string.IsNullOrWhiteSpace(elFinderParameter.Target)) // Search the target.
            {
                rootDirectory = await _assetRepository.Get(elFinderParameter.Target);
                if (rootDirectory == null)
                {
                    return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
                }
            }

            assets.Add(rootDirectory);
            foreach (var child in rootDirectory.Children)
            {
                assets.Add(child);
            }
            
            if (elFinderParameter.Tree)
            {
                var searchRootParameter = new SearchAssetsParameter // Search all the roots.
                {
                    AssetLevelType = AssetLevelTypes.ROOT,
                    ExcludedHashLst = assets.Select(a => a.Hash)
                };
                var searchResult = await _assetRepository.Search(searchRootParameter);
                foreach(var record in searchResult)
                {
                    assets.Add(record);
                    foreach(var child in record.Children)
                    {
                        if (assets.Any(a => a.Hash == child.Hash)) { continue; }
                        assets.Add(child);
                    }
                }
                if (!string.IsNullOrWhiteSpace(elFinderParameter.Target)) // Search the parents.
                {
                    var parents = await _assetRepository.GetAllParents(elFinderParameter.Target);
                    foreach(var parent in parents)
                    {
                        if (!assets.Any(a => a.Hash == parent.Hash))
                        {
                            assets.Add(parent);
                        }

                        foreach (var child in parent.Children)
                        {
                            if (assets.Any(a => a.Hash == child.Hash)) { continue; }
                            assets.Add(child);
                        }
                    }
                }
            }
            
            var files = new JArray();
            foreach(var asset in assets)
            {
                files.Add(GetFile(asset));
            }

            var result = new JObject(); // 3. Return the result.
            if (elFinderParameter.Init)
            {
                result.Add(Constants.ElFinderResponseNames.Api, "2.1");
            }

            var opts = new JObject();
            opts.Add(Constants.ElFinderOptionNames.Disabled, new JArray(new[] { "chmod" }));
            opts.Add(Constants.ElFinderOptionNames.Separator, Constants.PathSeparator);
            opts.Add(Constants.ElFinderOptionNames.Path, rootDirectory.Path);
            result.Add(Constants.ElFinderResponseNames.UplMaxSize, "0");
            result.Add(Constants.ElFinderResponseNames.Cwd, GetFile(rootDirectory));
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
            var target = elFinderParameter.Target;
            if (string.IsNullOrWhiteSpace(target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var asset = await _assetRepository.Get(target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var parents = await _assetRepository.GetAllParents(target);
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
            var target = elFinderParameter.Target;
            if (string.IsNullOrWhiteSpace(target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var asset = await _assetRepository.Get(target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
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
                Hash = HashHelper.GetHash(recordPath),
                MimeType=  Constants.MimeNames.Directory
            };

            if (!(await _assetRepository.Add(new[] { record })))
            {
                return new ErrorResponse(Constants.Errors.ErrInsertAsset).GetJson();
            }

            var files = new JArray(GetFile(record));
            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.Added, files);
            return result;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#rm
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteRm(ElFinderParameter elFinderParameter)
        {
            var targets = elFinderParameter.Targets;
            var assets = await _assetRepository.Search(new SearchAssetsParameter
            {
                HashLst = targets
            });

            if (assets.Count() != targets.Count())
            {
                return new ErrorResponse(Constants.Errors.ErrTargetsNotFound).GetJson();
            }

            if (!await _assetRepository.Remove(targets))
            {
                return new ErrorResponse(Constants.Errors.ErrRemoveAssets).GetJson();
            }

            var removed = new JArray(targets);
            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.Removed, removed);
            return result;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#rename
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteRename(ElFinderParameter elFinderParameter)
        {
            var target = elFinderParameter.Target;
            if (string.IsNullOrWhiteSpace(target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var asset = await _assetRepository.Get(target); // 1. Check the asset exists.
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var children = await _assetRepository.GetAllChildren(target); // 2. Get all the children & remove the assets.
            var assetIds = new List<string> { asset.Hash };
            assetIds.AddRange(children.Select(c => c.Hash));
            if (!await _assetRepository.Remove(assetIds))
            {
                return new ErrorResponse(Constants.Errors.ErrRemoveAssets).GetJson();
            }

            var previousPath = asset.Path; // 3. Update the path + hash.
            var splittedPath = asset.Path.Split(Constants.PathSeparator);
            int rootIndex = splittedPath.Count() - 1;
            splittedPath.SetValue(elFinderParameter.Name, rootIndex);
            var newPath = string.Join(Constants.PathSeparator.ToString(), splittedPath);
            asset.Name = elFinderParameter.Name;
            asset.Path = newPath;
            asset.Hash = HashHelper.GetHash(newPath);
            foreach(var child in children)
            {
                splittedPath = child.Path.Split(Constants.PathSeparator);
                splittedPath.SetValue(elFinderParameter.Name, rootIndex);
                var parentSplittedPath = splittedPath.Take(splittedPath.Count() - 1);
                newPath = string.Join(Constants.PathSeparator.ToString(), splittedPath);
                var parentPath = string.Join(Constants.PathSeparator.ToString(), parentSplittedPath);
                child.Path = newPath;
                child.Hash = HashHelper.GetHash(newPath);
                child.ResourceParentHash = HashHelper.GetHash(parentPath);
            }

            var newAssets = new List<AssetAggregate> { asset };
            newAssets.AddRange(children);
            if (!await _assetRepository.Add(newAssets))
            {
                return new ErrorResponse(Constants.Errors.ErrInsertAsset).GetJson();
            }

            var removed = new JArray(assetIds);
            var added = new JArray();
            foreach(var newAsset in newAssets)
            {
                added.Add(GetFile(newAsset));
            }

            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.Removed, removed);
            result.Add(Constants.ElFinderResponseNames.Added, added);
            return result;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#mkfile
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteMkfile(ElFinderParameter elFinderParameter)
        {
            var target = elFinderParameter.Target;
            if (string.IsNullOrWhiteSpace(target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var asset = await _assetRepository.Get(target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
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
                Hash = HashHelper.GetHash(recordPath),
                MimeType = Constants.MimeNames.TextPlain
            };

            if (!(await _assetRepository.Add(new[] { record })))
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

            return AssetResponse.Create(asset.Name, asset.Hash, Constants.VolumeId + "_", asset.Children.Any(), asset.ResourceParentHash,
                asset.MimeType, new AssetSecurity(asset.CanRead, asset.CanWrite, asset.IsLocked)).GetJson();
        }
    }
}
