using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SimpleIdentityServer.Client;
using SimpleIdentityServer.Client.DTOs.Response;
using SimpleIdentityServer.Manager.Client;
using SimpleIdentityServer.ResourceManager.API.Host.DTOs;
using SimpleIdentityServer.ResourceManager.API.Host.Helpers;
using SimpleIdentityServer.ResourceManager.Core.Models;
using SimpleIdentityServer.ResourceManager.Core.Parameters;
using SimpleIdentityServer.ResourceManager.Core.Repositories;
using SimpleIdentityServer.Uma.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebApiContrib.Core.Storage;

namespace SimpleIdentityServer.ResourceManager.API.Host.Controllers
{
    [Route(Constants.RouteNames.ElFinterController)]
    public class ElFinderController : Controller
    {
        private readonly IAssetRepository _assetRepository;
        private readonly IConfiguration _configuration;
        private readonly IOpenIdManagerClientFactory _openIdManagerClientFactory;
        private readonly IIdentityServerClientFactory _identityServerClientFactory;
        private readonly IIdentityServerUmaClientFactory _identityServerUmaClientFactory;
        private readonly IStorageHelper _storageHelper;
        private const string _umaWellKnownConfigurationName = "Uma:WellKnownConfiguration";
        private const string _openIdManagerWellKnownConfigurationName = "OpenIdManager:WellKnownConfiguration";
        private const string _openIdWellKnownConfigurationName = "OpenId:WellKnownConfiguration";
        private const string _clientIdConfigurationName = "OpenId:ClientId";
        private const string _clientSecretConfigurationName = "OpenId:ClientSecret";
        private const string _resourceManagerAccessToken = "resourceManagerAccessToken";
        private static IEnumerable<string> _scopes = new[] { "openid_manager", "uma_authorization", "uma_protection", "uma" };

        public ElFinderController(IAssetRepository assetRepository, IConfiguration configuration, IStorageHelper storageHelper,
            IOpenIdManagerClientFactory openIdManagerClientFactory, IIdentityServerClientFactory identityServerClientFactory,
            IIdentityServerUmaClientFactory identityServerUmaClientFactory)
        {
            _assetRepository = assetRepository;
            _configuration = configuration;
            _storageHelper = storageHelper;
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _identityServerClientFactory = identityServerClientFactory;
            _identityServerUmaClientFactory = identityServerUmaClientFactory;
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
                    return new OkObjectResult(await ExecuteOpen(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Parents:
                    return new OkObjectResult(await ExecuteParents(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Mkdir:
                    return new OkObjectResult(await ExecuteMkdir(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Rm:
                    return new OkObjectResult(await ExecuteRm(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Rename:
                    return new OkObjectResult(await ExecuteRename(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Mkfile:
                    return new OkObjectResult(await ExecuteMkfile(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Tree:
                    return new OkObjectResult(await ExecuteTree(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Duplicate:
                    return new OkObjectResult(await ExecuteDuplicate(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Paste:
                    return new OkObjectResult(await ExecutePaste(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Ls:
                    return new OkObjectResult(await ExecuteLs(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Search:
                    return new OkObjectResult(await ExecuteSearch(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Access:
                    return new OkObjectResult(await ExecuteAccess(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.Perms:
                    return new OkObjectResult(await ExecutePerms(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
                case ElFinderCommands.MkPerm:
                    return new OkObjectResult(await ExecuteMkPerm(deserializedParameter.ElFinderParameter).ConfigureAwait(false));
            }

            return new OkResult();
        }

        #region ELFINDER commands

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
                rootDirectory = await _assetRepository.Get(elFinderParameter.Target).ConfigureAwait(false);
            }

            if (elFinderParameter.Init || rootDirectory == null) // Returns the default root directory of the default volume.
            {
                var searchRootParameter = new SearchAssetsParameter
                {
                    AssetLevelType = AssetLevelTypes.ROOT,
                    IsDefaultWorkingDirectory = true
                };
                rootDirectory = (await _assetRepository.Search(searchRootParameter).ConfigureAwait(false)).First();
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
                var searchResult = await _assetRepository.Search(searchRootParameter).ConfigureAwait(false);
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
                    var parents = await _assetRepository.GetAllParents(elFinderParameter.Target).ConfigureAwait(false);
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

            var asset = await _assetRepository.Get(target).ConfigureAwait(false);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var parents = await _assetRepository.GetAllParents(target).ConfigureAwait(false);
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

            var asset = await _assetRepository.Get(target).ConfigureAwait(false);
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

            if (!(await _assetRepository.Add(new[] { record }).ConfigureAwait(false)))
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
            }).ConfigureAwait(false);

            if (assets.Count() != targets.Count())
            {
                return new ErrorResponse(Constants.Errors.ErrTargetsNotFound).GetJson();
            }

            if (!await _assetRepository.Remove(targets).ConfigureAwait(false))
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

            var asset = await _assetRepository.Get(target).ConfigureAwait(false); // 1. Check the asset exists.
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var children = await _assetRepository.GetAllChildren(target).ConfigureAwait(false); // 2. Get all the children & remove the assets.
            var assetIds = new List<string> { asset.Hash };
            assetIds.AddRange(children.Select(c => c.Hash));
            if (!await _assetRepository.Remove(assetIds).ConfigureAwait(false))
            {
                return new ErrorResponse(Constants.Errors.ErrRemoveAssets).GetJson();
            }

            Rename(asset, elFinderParameter.Name, children);
            var newAssets = new List<AssetAggregate> { asset };
            newAssets.AddRange(children);
            if (!await _assetRepository.Add(newAssets).ConfigureAwait(false))
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

            var asset = await _assetRepository.Get(target).ConfigureAwait(false);
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

            if (!(await _assetRepository.Add(new[] { record }).ConfigureAwait(false)))
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

            var targetedAsset = await _assetRepository.Get(elFinderParameter.Target).ConfigureAwait(false);
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

            var parents = await _assetRepository.GetAllParents(elFinderParameter.Target).ConfigureAwait(false);
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
            }).ConfigureAwait(false);

            if (targetAssets.Count() != elFinderParameter.Targets.Count())
            {
                return new ErrorResponse(Constants.Errors.ErrTargetsNotFound).GetJson();
            }

            var tasks = new List<Task<KeyValuePair<bool, IEnumerable<AssetAggregate>>>>();
            foreach (var targetAsset in targetAssets)
            {
                tasks.Add(Duplicate(targetAsset));
            }

            var result = await Task.WhenAll(tasks).ConfigureAwait(false);
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

            var sourceAsset = await _assetRepository.Get(elFinderParameter.Source).ConfigureAwait(false);
            if (sourceAsset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var destinationAsset = await _assetRepository.Get(elFinderParameter.Destination).ConfigureAwait(false);
            if (destinationAsset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }


            var assets = await _assetRepository.Search(new SearchAssetsParameter
            {
                HashLst = elFinderParameter.Targets
            }).ConfigureAwait(false);
            if (assets.Count() != elFinderParameter.Targets.Count())
            {
                return new ErrorResponse(Constants.Errors.ErrTargetsNotFound).GetJson();
            }

            var tasks = new List<Task<PasteOperation>>();
            foreach(var asset in assets)
            {
                tasks.Add(Copy(asset, sourceAsset, destinationAsset, elFinderParameter.Cut));
            }

            var tasksResult = await Task.WhenAll(tasks).ConfigureAwait(false);
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

            var targetAsset = await _assetRepository.Get(elFinderParameter.Target).ConfigureAwait(false);
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

            var target = elFinderParameter.Target;
            IEnumerable<AssetAggregate> assets;
            if (!string.IsNullOrWhiteSpace(target))
            {
                var targetAsset = await _assetRepository.Get(target).ConfigureAwait(false);
                if (targetAsset == null)
                {
                    return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
                }

                var children = await _assetRepository.GetAllChildren(target).ConfigureAwait(false);
                assets = children.Where(c => c.Name.Contains(elFinderParameter.Q));
            }
            else
            {
                assets = await _assetRepository.Search(new SearchAssetsParameter
                {
                    Names = new[] { elFinderParameter.Q }
                }).ConfigureAwait(false);
            }

            var files = new JArray();
            foreach(var asset in assets)
            {
                files.Add(GetFile(asset));
            }

            var jObj = new JObject();
            jObj.Add(Constants.ElFinderResponseNames.Files, files);
            return jObj;
        }

        #endregion

        #region UMA commands

        /// <summary>
        /// Returns information on how to access to the resource.
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteAccess(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var asset = await _assetRepository.Get(elFinderParameter.Target).ConfigureAwait(false);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var jObj = new JObject();
            jObj.Add(Constants.ElFinderResponseNames.Url, "http://localhost"); // TODO : Replace the url by the correct value.
            return jObj;
        }

        /// <summary>
        /// Get the permissions of the target / UMA resource.
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecutePerms(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var asset = await _assetRepository.Get(elFinderParameter.Target).ConfigureAwait(false);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var openIdWellKnownConfigurationUrl = GetWellKnownOpenIdConfigurationUrl();
            var umaWellKnownConfigurationUrl = GetWellKnownUmaConfigurationUrl();
            var grantedToken = await GetToken(_resourceManagerAccessToken, _scopes).ConfigureAwait(false);
            Func<Task<KeyValuePair<string, JArray>>> getDiscoveryInformation = new Func<Task<KeyValuePair<string, JArray>>>(async () =>
            {
                var openIdConfiguration = await _identityServerClientFactory.CreateDiscoveryClient().GetDiscoveryInformationAsync(openIdWellKnownConfigurationUrl).ConfigureAwait(false);
                return new KeyValuePair<string, JArray>(Constants.ElFinderResponseNames.OpenIdClaims, new JArray(openIdConfiguration.ClaimsSupported));
            });
            Func<Task<KeyValuePair<string, JArray>>> getOpenIdClients = new Func<Task<KeyValuePair<string, JArray>>>(async () =>
            {
                var jArr = new JArray();
                var openIdClients = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveGetAll(new Uri(GetWellKnownOpenIdManagerUrl()), grantedToken.AccessToken).ConfigureAwait(false);
                foreach (var openIdClient in openIdClients)
                {
                    var record = new JObject();
                    record.Add(Constants.ElFinderOpenIdClientResponseNames.ClientId, openIdClient.ClientId);
                    record.Add(Constants.ElFinderOpenIdClientResponseNames.ClientName, openIdClient.ClientName);
                    record.Add(Constants.ElFinderOpenIdClientResponseNames.LogoUri, openIdClient.LogoUri);
                    jArr.Add(record);
                }

                return new KeyValuePair<string, JArray>(Constants.ElFinderResponseNames.OpenIdClients, jArr);
            });
            Func<Task<KeyValuePair<string, JArray>>> getAuthPolicyRules = new Func<Task<KeyValuePair<string, JArray>>>(async () =>
            {
                PolicyResponse policyResponse = null;
                if (asset.AuthorizationPolicies != null && asset.AuthorizationPolicies.Any(a => a.IsOwner))
                {
                    var authorizationPolicy = asset.AuthorizationPolicies.First(a => a.IsOwner).AuthPolicyId;
                    policyResponse = await _identityServerUmaClientFactory.GetPolicyClient().GetByResolution(authorizationPolicy, umaWellKnownConfigurationUrl, grantedToken.AccessToken).ConfigureAwait(false); // Retrieve the authorization policy.
                }

                var jArrPolicyRules = new JArray();
                if (policyResponse != null)
                {
                    foreach (var policyRule in policyResponse.Rules)
                    {
                        var record = new JObject();
                        record.Add(Constants.ElFinderUmaAuthorizationPolicyRuleNames.ClientIdsAllowed, new JArray(policyRule.ClientIdsAllowed));
                        record.Add(Constants.ElFinderUmaAuthorizationPolicyRuleNames.Id, policyRule.Id);
                        record.Add(Constants.ElFinderUmaAuthorizationPolicyRuleNames.IsResourceOwnerConsentNeeded, policyRule.IsResourceOwnerConsentNeeded);
                        record.Add(Constants.ElFinderUmaAuthorizationPolicyRuleNames.Scopes, new JArray(policyRule.Scopes));
                        record.Add(Constants.ElFinderUmaAuthorizationPolicyRuleNames.Script, policyRule.Script);
                        var claims = new JArray();
                        foreach (var cl in policyRule.Claims)
                        {
                            var claim = new JObject();
                            claim.Add(Constants.ElFinderClaimNames.Type, cl.Type);
                            claim.Add(Constants.ElFinderClaimNames.Value, cl.Value);
                            claims.Add(claim);
                        }

                        record.Add(Constants.ElFinderUmaAuthorizationPolicyRuleNames.Claims, claims);
                        jArrPolicyRules.Add(record);
                    }
                }

                return new KeyValuePair<string, JArray>(Constants.ElFinderResponseNames.AuthRules, jArrPolicyRules);
            });
            var tasks = new List<Task<KeyValuePair<string, JArray>>>
            {
                getDiscoveryInformation(),
                getOpenIdClients(),
                getAuthPolicyRules()
            };
            var tasksResult = await Task.WhenAll(tasks).ConfigureAwait(false);
            var result = new JObject();
            var permissions = new JArray(new List<string> { "read", "write", "execute" });
            foreach(var kvp in tasksResult)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            result.Add(Constants.ElFinderResponseNames.Permissions, permissions);
            return result;
        }

        /// <summary>
        /// Create a permission.
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteMkPerm(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            if (elFinderParameter.Rules == null)
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Rules)).GetJson();
            }

            var asset = await _assetRepository.Get(elFinderParameter.Target).ConfigureAwait(false);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var authPolRules = new List<AddAuthRuleParameter>();
            foreach (JObject rule in elFinderParameter.Rules) // Extract the rules.
            {
                var record = new AddAuthRuleParameter();
                JToken jtRuleId;
                if (rule.TryGetValue(Constants.ElFinderAuthPolRuleNames.Id, out jtRuleId))
                {
                    record.RuleId = jtRuleId.ToString();
                }

                JToken jtScopes;
                if (rule.TryGetValue(Constants.ElFinderAuthPolRuleNames.Scopes, out jtScopes))
                {
                    var jtScopesArr = jtScopes as JArray;
                    if (jtScopesArr != null)
                    {
                        record.OpenIdScopes = jtScopesArr.ToObject<List<string>>();
                    }
                }

                JToken jtOpenIdClients;
                if (rule.TryGetValue(Constants.ElFinderAuthPolRuleNames.OpenIdClients, out jtOpenIdClients))
                {
                    var jtOpenIdClientsArr = jtOpenIdClients as JArray;
                    if (jtOpenIdClientsArr != null)
                    {
                        record.OpenIdClients = jtOpenIdClientsArr.ToObject<List<string>>();
                    }
                }

                JToken jtClaims;
                if (rule.TryGetValue(Constants.ElFinderAuthPolRuleNames.Claims, out jtClaims))
                {
                    var jtClaimsArr = jtClaims as JArray;
                    var claims = new List<AddAuthRuleClaimParameter>();
                    foreach(JObject jtClaim in jtClaims)
                    {
                        var rec = new AddAuthRuleClaimParameter
                        {
                            Type = jtClaim.GetValue(Constants.ElFinderClaimNames.Type).ToString(),
                            Value = jtClaim.GetValue(Constants.ElFinderClaimNames.Value).ToString()
                        };

                        claims.Add(rec);
                    }

                    record.Claims = claims;
                }

                authPolRules.Add(record);
            }

            var openIdWellKnownConfigurationUrl = GetWellKnownOpenIdConfigurationUrl();
            var umaWellKnownConfigurationUrl = GetWellKnownUmaConfigurationUrl();
            var grantedToken = await GetToken(_resourceManagerAccessToken, _scopes).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(asset.ResourceId)) // CREATE AN UMA RESOURCE.
            {
                var addedResource = await _identityServerUmaClientFactory.GetResourceSetClient().AddByResolution(new PostResourceSet
                {
                    Name = asset.Name,
                    Scopes = new List<string> { "read", "write", "execute" }
                }, umaWellKnownConfigurationUrl, grantedToken.AccessToken).ConfigureAwait(false);
                asset.ResourceId = addedResource.Id;
                if (!await _assetRepository.Update(new[] { asset }).ConfigureAwait(false))
                {
                    // TODO : RETURNS AN ERROR.
                }
            }

            if (asset.AuthorizationPolicies == null || !asset.AuthorizationPolicies.Any(ap => ap.IsOwner)) // Add an authorization policy.
            {
                AddPolicyResponse addPolicyResponse = await _identityServerUmaClientFactory.GetPolicyClient().AddByResolution(new PostPolicy
                {
                    ResourceSetIds = new List<string> { asset.ResourceId },
                    Rules = authPolRules.Select(apr =>
                        new PostPolicyRule
                        {
                            ClientIdsAllowed = apr.OpenIdClients.ToList(),
                            Scopes = apr.OpenIdScopes.ToList(),
                            Claims = apr.Claims == null ? null : apr.Claims.Select(c => new PostClaim
                            {
                                Type = c.Type,
                                Value = c.Value
                            }).ToList(),
                            Script = string.Empty,
                            IsResourceOwnerConsentNeeded = false
                        }
                    ).ToList()
                }, umaWellKnownConfigurationUrl, grantedToken.AccessToken).ConfigureAwait(false);
                asset.AuthorizationPolicies.Add(new AssetAggregateAuthPolicy
                {
                    IsOwner = true,
                    AuthPolicyId = addPolicyResponse.PolicyId
                });
                var lstAssets = new List<AssetAggregate> { asset };
                var children = await _assetRepository.GetAllChildren(asset.Hash).ConfigureAwait(false);
                foreach(var child in children)
                {
                    child.AuthorizationPolicies.Add(new AssetAggregateAuthPolicy
                    {
                        IsOwner = false,
                        AuthPolicyId = addPolicyResponse.PolicyId
                    });
                }

                lstAssets.AddRange(children);
                if (!await _assetRepository.Update(lstAssets).ConfigureAwait(false))
                {
                    // TODO : Returns an error.
                }
            }
            else // Update the authorization policy.
            {
                var authPolicy = asset.AuthorizationPolicies.First(ap => ap.IsOwner);
                await _identityServerUmaClientFactory.GetPolicyClient().UpdateByResolution(new PutPolicy
                {
                    PolicyId = authPolicy.AuthPolicyId,
                    Rules = authPolRules.Select(apr => 
                        new PutPolicyRule
                        {
                            Id = apr.RuleId,
                            ClientIdsAllowed = apr.OpenIdClients.ToList(),
                            Scopes = apr.OpenIdScopes.ToList(),
                            Claims = apr.Claims == null ? null : apr.Claims.Select(c => new PostClaim
                            {
                                Type = c.Type,
                                Value = c.Value
                            }).ToList(),
                            Script = string.Empty,
                            IsResourceOwnerConsentNeeded = false
                        }
                    ).ToList()
                }, umaWellKnownConfigurationUrl, grantedToken.AccessToken).ConfigureAwait(false);
            }
            
            // TODO : Deserialize the rules & do the necessary to insert the permissions.
            var jObj = new JObject();
            return jObj;
        }

        #endregion

        #region OPENID manager commands

        /// <summary>
        /// Get all the openid clients.
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteOpenIdClients(ElFinderParameter elFinderParameter)
        {
            var grantedToken = await GetToken(_resourceManagerAccessToken, _scopes).ConfigureAwait(false);
            var openIdClients = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveGetAll(new Uri(GetWellKnownOpenIdManagerUrl()), grantedToken.AccessToken).ConfigureAwait(false);
            var jArr = new JArray();
            foreach(var openIdClient in openIdClients)
            {
                var jObj = new JObject();
                jObj.Add(Constants.ElFinderOpenIdClientResponseNames.ClientId, openIdClient.ClientId);
                jObj.Add(Constants.ElFinderOpenIdClientResponseNames.ClientName, openIdClient.ClientName);
                jObj.Add(Constants.ElFinderOpenIdClientResponseNames.LogoUri, openIdClient.LogoUri);
                jArr.Add(jObj);
            }

            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.OpenIdClients, jArr);
            return result;
        }

        #endregion

        private async Task<PasteOperation> Copy(AssetAggregate asset, AssetAggregate source, AssetAggregate target, bool isCut = false)
        {
            var children = await _assetRepository.GetAllChildren(asset.Hash).ConfigureAwait(false);
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
            if (!await _assetRepository.Add(newAssets).ConfigureAwait(false))
            {
                return new PasteOperation(true);
            }

            if (isCut)
            {
                if (!await _assetRepository.Remove(hashLst).ConfigureAwait(false))
                {
                    return new PasteOperation(true);
                }
            }

            return new PasteOperation(!isCut ? new List<string>() : hashLst, newAssets);
        }

        private async Task<GrantedToken> GetToken(string key, IEnumerable<string> scopes)
        {
            GrantedToken grantedToken = null;
            var clientId = GetClientId();
            var clientSecret = GetClientSecret();
            var wellKnownConfiguration = GetWellKnownOpenIdConfigurationUrl();
            var storageRecord = await _storageHelper.GetAsync<GrantedToken>(key).ConfigureAwait(false);
            var clientSelector = _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth(clientId, clientSecret);
            if (storageRecord != null && storageRecord.Obj != null)
            {
                var currentDate = DateTime.UtcNow;
                var expirationDate = DateTime.UtcNow.AddSeconds(storageRecord.Obj.ExpiresIn);
                if (currentDate >= expirationDate) // Check expiration.
                {
                    var refreshToken = storageRecord.Obj.RefreshToken;
                    await _storageHelper.SetAsync<GrantedToken>(key, null).ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(storageRecord.Obj.RefreshToken)) // Get an access token by using the refresh token.
                    {
                        try
                        {
                            grantedToken = await clientSelector.UseRefreshToken(storageRecord.Obj.RefreshToken).ResolveAsync(wellKnownConfiguration).ConfigureAwait(false);
                        }
                        catch (Exception) { }
                    }
                }
                else
                {
                    grantedToken = storageRecord.Obj;
                }
            }

            if (grantedToken != null)
            {
                return grantedToken;
            }
            
            grantedToken = await _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth(clientId, clientSecret)
                .UseClientCredentials(scopes.ToArray())
                .ResolveAsync(wellKnownConfiguration).ConfigureAwait(false);
            await _storageHelper.SetAsync(key, grantedToken).ConfigureAwait(false);
            return grantedToken;
        }

        private async Task<KeyValuePair<bool, IEnumerable<AssetAggregate>>> Duplicate(AssetAggregate asset)
        {
            var children = await _assetRepository.GetAllChildren(asset.Hash).ConfigureAwait(false);
            Rename(asset, asset.Name.Split('_').First() + "_" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture), children);
            var newAssets = new List<AssetAggregate> { asset };
            newAssets.AddRange(children);
            if (!await _assetRepository.Add(newAssets).ConfigureAwait(false))
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

        private string GetWellKnownUmaConfigurationUrl()
        {
            return _configuration[_umaWellKnownConfigurationName];
        }

        private string GetWellKnownOpenIdManagerUrl()
        {
            return _configuration[_openIdManagerWellKnownConfigurationName];
        }

        private string GetWellKnownOpenIdConfigurationUrl()
        {
            return _configuration[_openIdWellKnownConfigurationName];
        }

        private string GetClientId()
        {
            return _configuration[_clientIdConfigurationName];
        }

        private string GetClientSecret()
        {

            return _configuration[_clientSecretConfigurationName];
        }
    }
}
