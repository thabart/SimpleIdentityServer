using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.ResourceManager.API.Host.DTOs;
using SimpleIdentityServer.ResourceManager.API.Host.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                case ElFinderCommands.Tree:
                    return new OkObjectResult(await ExecuteTree(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Duplicate:
                    return new OkObjectResult(await ExecuteDuplicate(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Paste:
                    return new OkObjectResult(await ExecutePaste(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Ls:
                    return new OkObjectResult(await ExecuteLs(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Search:
                    return new OkObjectResult(await ExecuteSearch(deserializedParameter.ElFinderParameter));
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
            if (!string.IsNullOrWhiteSpace(elFinderParameter.Target)) // Search the target.
            {
                rootDirectory = await _assetRepository.Get(elFinderParameter.Target);
            }

            if (elFinderParameter.Init || rootDirectory == null) // Returns the default root directory of the default volume.
            {
                var searchRootParameter = new SearchAssetsParameter
                {
                    AssetLevelType = AssetLevelTypes.ROOT,
                    IsDefaultWorkingDirectory = true
                };
                rootDirectory = (await _assetRepository.Search(searchRootParameter)).First();
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

            var recordPath = asset.Path + Constants.PathSeparator + elFinderParameter.Name;
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

            Rename(asset, elFinderParameter.Name, children);
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

            var recordPath = asset.Path + Constants.PathSeparator + elFinderParameter.Name;
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

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#tree
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteTree(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var targetedAsset = await _assetRepository.Get(elFinderParameter.Target);
            if (targetedAsset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }
                       
            var assets = new List<AssetAggregate>();
            assets.Add(targetedAsset);
            foreach (var child in targetedAsset.Children)
            {
                assets.Add(child);
            }

            var parents = await _assetRepository.GetAllParents(elFinderParameter.Target);
            foreach (var parent in parents)
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
            
            var files = new JArray();
            foreach (var asset in assets)
            {
                files.Add(GetFile(asset));
            }

            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.Tree, files);
            return result;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#duplicate
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteDuplicate(ElFinderParameter elFinderParameter)
        {
            if (elFinderParameter.Targets == null)
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Targets)).GetJson();
            }

            var targetAssets = await _assetRepository.Search(new SearchAssetsParameter
            {
                HashLst = elFinderParameter.Targets
            });

            if (targetAssets.Count() != elFinderParameter.Targets.Count())
            {
                return new ErrorResponse(Constants.Errors.ErrTargetsNotFound).GetJson();
            }

            var tasks = new List<Task<KeyValuePair<bool, IEnumerable<AssetAggregate>>>>();
            foreach (var targetAsset in targetAssets)
            {
                tasks.Add(Duplicate(targetAsset));
            }

            var result = await Task.WhenAll(tasks);
            if (result.Any(b => !b.Key))
            {
                return new ErrorResponse(Constants.Errors.ErrDuplicateAsset).GetJson();
            }
            
            var files = new JArray();
            foreach(var kvp in result)
            {
                foreach(var assetAgg in kvp.Value)
                {
                    files.Add(GetFile(assetAgg));
                }
            }

            var resp = new JObject();
            resp.Add(Constants.ElFinderResponseNames.Added, files);
            return resp;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#paste
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecutePaste(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Source))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Source)).GetJson();
            }

            if (string.IsNullOrWhiteSpace(elFinderParameter.Destination))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Destination)).GetJson();
            }

            if (elFinderParameter.Targets == null)
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Targets)).GetJson();
            }

            var sourceAsset = await _assetRepository.Get(elFinderParameter.Source);
            if (sourceAsset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var destinationAsset = await _assetRepository.Get(elFinderParameter.Destination);
            if (destinationAsset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }


            var assets = await _assetRepository.Search(new SearchAssetsParameter
            {
                HashLst = elFinderParameter.Targets
            });
            if (assets.Count() != elFinderParameter.Targets.Count())
            {
                return new ErrorResponse(Constants.Errors.ErrTargetsNotFound).GetJson();
            }

            var tasks = new List<Task<PasteOperation>>();
            foreach(var asset in assets)
            {
                tasks.Add(Copy(asset, sourceAsset, destinationAsset, elFinderParameter.Cut));
            }

            var tasksResult = await Task.WhenAll(tasks);
            if (tasksResult.Any(t => t.IsError))
            {
                if (elFinderParameter.Cut)
                {
                    return new ErrorResponse(Constants.Errors.ErrCutAsset).GetJson();
                }
                else
                {
                    return new ErrorResponse(Constants.Errors.ErrPasteAsset).GetJson();
                }
            }

            var added = new JArray();
            var removed = new JArray();
            foreach (var kvp in tasksResult)
            {
                foreach(var r in kvp.Removed)
                {
                    removed.Add(r);
                }

                foreach (var a in kvp.Added)
                {
                    added.Add(GetFile(a));
                }
            }

            var jObj = new JObject();
            jObj.Add(Constants.ElFinderResponseNames.Added, added);
            jObj.Add(Constants.ElFinderResponseNames.Removed, removed);
            return jObj;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#ls
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteLs(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var targetAsset = await _assetRepository.Get(elFinderParameter.Target);
            if (targetAsset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var jObj = new JObject();
            jObj.Add(Constants.ElFinderResponseNames.List, new JArray(targetAsset.Children.Select(c => c.Name)));    
            return jObj;
        }

        /// <summary>
        /// https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#search
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteSearch(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Q))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Q)).GetJson();
            }

            var assets = await _assetRepository.Search(new SearchAssetsParameter
            {
                Names = new [] { elFinderParameter.Q }
            });
            var files = new JArray();
            foreach(var asset in assets)
            {
                files.Add(GetFile(asset));
            }

            var jObj = new JObject();
            jObj.Add(Constants.ElFinderResponseNames.Files, files);
            return jObj;
        }
        
        private async Task<PasteOperation> Copy(AssetAggregate asset, AssetAggregate source, AssetAggregate target, bool isCut = false)
        {
            var children = await _assetRepository.GetAllChildren(asset.Hash);
            var hashLst = new List<string> { asset.Hash };
            hashLst.AddRange(children.Select(c => c.Hash));
            var previousPath = target.Path;
            var newPath = target.Path + Constants.PathSeparator + asset.Name;
            asset.Path = newPath;
            asset.Hash = HashHelper.GetHash(asset.Path);
            asset.ResourceParentHash = target.Hash;

            foreach (var child in children)
            {
                var childPath = child.Path.Replace(source.Path, target.Path);
                var splittedPath = childPath.Split(Constants.PathSeparator);
                var parentPath = string.Join(Constants.PathSeparator.ToString(), splittedPath.Take(splittedPath.Count() - 1));
                child.Path = childPath;
                child.Hash = HashHelper.GetHash(childPath);
                child.ResourceParentHash = HashHelper.GetHash(parentPath);
            }

            var newAssets = new List<AssetAggregate> { asset };
            newAssets.AddRange(children);
            if (!await _assetRepository.Add(newAssets))
            {
                return new PasteOperation(true);
            }

            if (isCut)
            {
                if (!await _assetRepository.Remove(hashLst))
                {
                    return new PasteOperation(true);
                }
            }

            return new PasteOperation(!isCut ? new List<string>() : hashLst, newAssets);
        }

        private async Task<KeyValuePair<bool, IEnumerable<AssetAggregate>>> Duplicate(AssetAggregate asset)
        {
            var children = await _assetRepository.GetAllChildren(asset.Hash);
            Rename(asset, asset.Name.Split('_').First() + "_" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), children);
            var newAssets = new List<AssetAggregate> { asset };
            newAssets.AddRange(children);
            if (!await _assetRepository.Add(newAssets))
            {
                return new KeyValuePair<bool, IEnumerable<AssetAggregate>>(false, null);
            }

            return new KeyValuePair<bool, IEnumerable<AssetAggregate>>(true, newAssets); ;
        }

        private void Rename(AssetAggregate asset, string newName, IEnumerable<AssetAggregate> children)
        {
            var previousPath = asset.Path;
            var splittedPath = asset.Path.Split(Constants.PathSeparator);
            int rootIndex = splittedPath.Count() - 1;
            splittedPath.SetValue(newName, rootIndex);
            var newPath = string.Join(Constants.PathSeparator.ToString(), splittedPath);
            asset.Name = newName;
            asset.Path = newPath;
            asset.Hash = HashHelper.GetHash(newPath);
            foreach (var child in children)
            {
                splittedPath = child.Path.Split(Constants.PathSeparator);
                splittedPath.SetValue(newName, rootIndex);
                var parentSplittedPath = splittedPath.Take(splittedPath.Count() - 1);
                newPath = string.Join(Constants.PathSeparator.ToString(), splittedPath);
                var parentPath = string.Join(Constants.PathSeparator.ToString(), parentSplittedPath);
                child.Path = newPath;
                child.Hash = HashHelper.GetHash(newPath);
                child.ResourceParentHash = HashHelper.GetHash(parentPath);
            }
        }

        private class PasteOperation
        {
            public PasteOperation(bool isError)
            {
                IsError = isError;
            }

            public PasteOperation(IEnumerable<string> removed, IEnumerable<AssetAggregate> added)
            {
                Removed = removed;
                Added = added;
            }

            public IEnumerable<string> Removed;
            public IEnumerable<AssetAggregate> Added;
            public bool IsError;
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
