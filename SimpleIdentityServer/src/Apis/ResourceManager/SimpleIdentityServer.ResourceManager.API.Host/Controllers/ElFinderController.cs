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
        private readonly IIdProviderRepository _idProviderRepository;
        private readonly IStorageHelper _storageHelper;
        private const string _authKnownConfigurationName = "Auth:WellKnownConfiguration";
        private const string _clientIdConfigurationName = "Auth:ClientId";
        private const string _clientSecretConfigurationName = "Auth:ClientSecret";
        private const string _resourceManagerAccessToken = "resourceManagerAccessToken";
        private static IEnumerable<string> _scopes = new[] { "uma_protection" };

        public ElFinderController(IAssetRepository assetRepository, IConfiguration configuration, IStorageHelper storageHelper,
            IOpenIdManagerClientFactory openIdManagerClientFactory, IIdentityServerClientFactory identityServerClientFactory,
            IIdentityServerUmaClientFactory identityServerUmaClientFactory, IIdProviderRepository idProviderRepository)
        {
            _assetRepository = assetRepository;
            _configuration = configuration;
            _storageHelper = storageHelper;
            _openIdManagerClientFactory = openIdManagerClientFactory;
            _identityServerClientFactory = identityServerClientFactory;
            _identityServerUmaClientFactory = identityServerUmaClientFactory;
            _idProviderRepository = idProviderRepository;
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
                case ElFinderCommands.Access:
                    return new OkObjectResult(await ExecuteAccess(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.Perms:
                    return new OkObjectResult(await ExecutePerms(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.MkPerm:
                    return new OkObjectResult(await ExecuteMkPerm(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.GetResource:
                    return new OkObjectResult(await ExecuteGetResource(deserializedParameter.ElFinderParameter));
                case ElFinderCommands.PatchResource:
                    return new OkObjectResult(await ExecutePatchResource(deserializedParameter.ElFinderParameter));
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

            var target = elFinderParameter.Target;
            IEnumerable<AssetAggregate> assets;
            if (!string.IsNullOrWhiteSpace(target))
            {
                var targetAsset = await _assetRepository.Get(target);
                if (targetAsset == null)
                {
                    return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
                }

                var children = await _assetRepository.GetAllChildren(target);
                assets = children.Where(c => c.Name.Contains(elFinderParameter.Q));
            }
            else
            {
                assets = await _assetRepository.Search(new SearchAssetsParameter
                {
                    Names = new[] { elFinderParameter.Q }
                });
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

            var asset = await _assetRepository.Get(elFinderParameter.Target);
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

            var asset = await _assetRepository.Get(elFinderParameter.Target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            if (string.IsNullOrWhiteSpace(asset.ResourceId))
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrNoResource).GetJson();
            }

            var authWellKnownConfigurationUrl = GetWellKnownAuthConfigurationUrl();
            var grantedToken = await GetToken(_resourceManagerAccessToken, _scopes);
            var resourceInformation = await _identityServerUmaClientFactory.GetResourceSetClient().GetByResolution(asset.ResourceId, authWellKnownConfigurationUrl, grantedToken.AccessToken);
            if (resourceInformation == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrNoResource).GetJson();
            }
            var permissions = new JArray();
            if (resourceInformation.Scopes != null)
            {
                foreach (var scope in resourceInformation.Scopes)
                {
                    permissions.Add(scope);
                }
            }

            var idProviders = await _idProviderRepository.GetAll();
            var jArrIdProviders = new JArray();
            foreach (var idProvider in idProviders)
            {
                var jObjIdProvider = new JObject();
                jObjIdProvider.Add(Constants.ElFinderIdProviderResponseNames.Url, idProvider.OpenIdWellKnownUrl);
                jObjIdProvider.Add(Constants.ElFinderIdProviderResponseNames.Description, idProvider.Description);
                jObjIdProvider.Add(Constants.ElFinderIdProviderResponseNames.Name, idProvider.Name);
                jArrIdProviders.Add(jObjIdProvider);
            }

            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.Permissions, permissions);
            result.Add(Constants.ElFinderResponseNames.IdProviders, jArrIdProviders);
            if (asset.AuthorizationPolicies !=  null && asset.AuthorizationPolicies.Any())
            {
                var authorizationPolicy = asset.AuthorizationPolicies.First().AuthPolicyId;
                var policyResponse = await _identityServerUmaClientFactory.GetPolicyClient().GetByResolution(authorizationPolicy, authWellKnownConfigurationUrl, grantedToken.AccessToken);
                var jArrPolicyRules = new JArray();
                if (policyResponse != null)
                {
                    foreach (var policyRule in policyResponse.Rules)
                    {
                        var record = new JObject();
                        record.Add(Constants.ElFinderAuthPolRuleNames.OpenIdClients, new JArray(policyRule.ClientIdsAllowed));
                        record.Add(Constants.ElFinderAuthPolRuleNames.Id, policyRule.Id);
                        record.Add(Constants.ElFinderAuthPolRuleNames.Permissions, new JArray(policyRule.Scopes));
                        var claims = new JArray();
                        foreach (var cl in policyRule.Claims)
                        {
                            var claim = new JObject();
                            claim.Add(Constants.ElFinderClaimNames.Type, cl.Type);
                            claim.Add(Constants.ElFinderClaimNames.Value, cl.Value);
                            claims.Add(claim);
                        }

                        record.Add(Constants.ElFinderAuthPolRuleNames.OpenIdClaims, claims);
                        jArrPolicyRules.Add(record);
                    }
                }

                result.Add(Constants.ElFinderResponseNames.AuthRules, jArrPolicyRules);
            }

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

            var asset = await _assetRepository.Get(elFinderParameter.Target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            if (string.IsNullOrWhiteSpace(asset.ResourceId))
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrNoResource).GetJson();
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
                if (rule.TryGetValue(Constants.ElFinderAuthPolRuleNames.Permissions, out jtScopes))
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
                if (rule.TryGetValue(Constants.ElFinderAuthPolRuleNames.OpenIdClaims, out jtClaims))
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
            
            var authWellKnownConfigurationUrl = GetWellKnownAuthConfigurationUrl();
            var grantedToken = await GetToken(_resourceManagerAccessToken, _scopes);
            if (asset.AuthorizationPolicies == null || !asset.AuthorizationPolicies.Any()) // Add an authorization policy.
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
                }, authWellKnownConfigurationUrl, grantedToken.AccessToken);
                asset.AuthorizationPolicies.Add(new AssetAggregateAuthPolicy
                {
                    AuthPolicyId = addPolicyResponse.PolicyId
                });
                var lstAssets = new List<AssetAggregate> { asset };
                if (!await _assetRepository.Update(lstAssets))
                {
                    return new ErrorResponse(Constants.ElFinderErrors.ErrUpdateResource).GetJson();
                }
            }
            else // Update the authorization policy.
            {
                var authPolicy = asset.AuthorizationPolicies.First();
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
                }, authWellKnownConfigurationUrl, grantedToken.AccessToken);
            }
            
            var jObj = new JObject();
            return jObj;
        }

        /// <summary>
        /// Get the resource information.
        /// </summary>
        /// <param name="elFinderParameter"></param>
        /// <returns></returns>
        private async Task<JObject> ExecuteGetResource(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var asset = await _assetRepository.Get(elFinderParameter.Target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }
            
            var umaWellKnownConfigurationUrl = GetWellKnownAuthConfigurationUrl();
            if (string.IsNullOrWhiteSpace(asset.ResourceId))
            {
                var jObj = new JObject();
                jObj.Add(Constants.ElFinderResponseNames.Resource, string.Empty);
                return jObj;
            }

            var grantedToken = await GetToken(_resourceManagerAccessToken, _scopes);
            var resourceInformation = await _identityServerUmaClientFactory.GetResourceSetClient().GetByResolution(asset.ResourceId, umaWellKnownConfigurationUrl, grantedToken.AccessToken);
            var content = new JObject();
            content.Add(Constants.ElFinderResourceNames.Id, resourceInformation.Id);
            content.Add(Constants.ElFinderResourceNames.IconUri, resourceInformation.IconUri);
            content.Add(Constants.ElFinderResourceNames.Name, resourceInformation.Name);
            var scopes = new JArray();
            if (resourceInformation.Scopes != null)
            {
                foreach(var scope in resourceInformation.Scopes)
                {
                    scopes.Add(scope);
                }
            }

            content.Add(Constants.ElFinderResourceNames.Scopes, scopes);
            content.Add(Constants.ElFinderResourceNames.Type, resourceInformation.Type);
            var result = new JObject();
            result.Add(Constants.ElFinderResponseNames.Resource, content);
            return result;
        }

        private async Task<JObject> ExecutePatchResource(ElFinderParameter elFinderParameter)
        {
            if (string.IsNullOrWhiteSpace(elFinderParameter.Target))
            {
                return new ErrorResponse(string.Format(Constants.Errors.ErrParamNotSpecified, Constants.ElFinderDtoNames.Target)).GetJson();
            }

            var asset = await _assetRepository.Get(elFinderParameter.Target);
            if (asset == null)
            {
                return new ErrorResponse(Constants.ElFinderErrors.ErrTrgFolderNotFound).GetJson();
            }

            var umaWellKnownConfigurationUrl = GetWellKnownAuthConfigurationUrl();
            var grantedToken = await GetToken(_resourceManagerAccessToken, _scopes);
            if (string.IsNullOrWhiteSpace(asset.ResourceId))
            {
                var addedResource = await _identityServerUmaClientFactory.GetResourceSetClient().AddByResolution(new PostResourceSet
                {
                    Name = asset.Name,
                    Scopes = elFinderParameter.Scopes.ToList()
                }, umaWellKnownConfigurationUrl, grantedToken.AccessToken);
                asset.ResourceId = addedResource.Id;
                if (!await _assetRepository.Update(new[] { asset }))
                {
                    return new ErrorResponse(Constants.ElFinderErrors.ErrCreateResource).GetJson();
                }

                return new JObject();
            }

            await _identityServerUmaClientFactory.GetResourceSetClient().UpdateByResolution(new PutResourceSet
            {
                Id = asset.ResourceId,
                Name = asset.Name,
                Scopes = elFinderParameter.Scopes.ToList()
            }, umaWellKnownConfigurationUrl, grantedToken.AccessToken);
            return new JObject();
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
            /*
            var grantedToken = await GetToken(_resourceManagerAccessToken, _scopes);
            var openIdClients = await _openIdManagerClientFactory.GetOpenIdsClient().ResolveGetAll(new Uri(GetWellKnownOpenIdManagerUrl()), grantedToken.AccessToken);
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
            */
            return null;
        }

        #endregion

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

        private async Task<GrantedToken> GetToken(string key, IEnumerable<string> scopes)
        {
            GrantedToken grantedToken = null;
            var clientId = GetClientId();
            var clientSecret = GetClientSecret();
            var wellKnownConfiguration = GetWellKnownAuthConfigurationUrl();
            var storageRecord = await _storageHelper.GetAsync<GrantedToken>(key);
            var clientSelector = _identityServerClientFactory.CreateAuthSelector()
                .UseClientSecretPostAuth(clientId, clientSecret);
            if (storageRecord != null && storageRecord.Obj != null)
            {
                var currentDate = DateTime.UtcNow;
                var expirationDate = DateTime.UtcNow.AddSeconds(storageRecord.Obj.ExpiresIn);
                if (currentDate >= expirationDate) // Check expiration.
                {
                    var refreshToken = storageRecord.Obj.RefreshToken;
                    await _storageHelper.SetAsync<GrantedToken>(key, null);
                    if (!string.IsNullOrWhiteSpace(storageRecord.Obj.RefreshToken)) // Get an access token by using the refresh token.
                    {
                        try
                        {
                            grantedToken = await clientSelector.UseRefreshToken(storageRecord.Obj.RefreshToken).ResolveAsync(wellKnownConfiguration);
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
                .ResolveAsync(wellKnownConfiguration);
            await _storageHelper.SetAsync(key, grantedToken);
            return grantedToken;
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
                asset.MimeType, new AssetSecurity(asset.CanRead, asset.CanWrite, asset.IsLocked, asset.AuthorizationPolicies != null && asset.AuthorizationPolicies.Any())).GetJson();
        }

        private string GetWellKnownAuthConfigurationUrl()
        {
            return _configuration[_authKnownConfigurationName];
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
