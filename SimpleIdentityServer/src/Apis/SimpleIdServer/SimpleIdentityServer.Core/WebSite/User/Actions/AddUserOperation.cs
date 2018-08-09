#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using SimpleIdentityServer.AccessToken.Store;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.OpenId.Logging;
using SimpleIdentityServer.Scim.Client;
using SimpleIdentityServer.AccountFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IAddUserOperation
    {
        Task<bool> Execute(AddUserParameter addUserParameter, AuthenticationParameter authenticationParameter, string scimBaseUrl = null, bool addScimResource = false, string issuer = null);
    }
    
    public class AddUserOperation : IAddUserOperation
    {
        private class ScimUser
        {
            public ScimUser(string id, string url)
            {
                Id = id;
                Url = url;
            }

            public string Id { get; set; }
            public string Url { get; set; }
        }

        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IAccessTokenStore _tokenStore;
        private readonly IScimClientFactory _scimClientFactory;
        private readonly IEnumerable<IAccountFilter> _accountFilters;
        private readonly ILinkProfileAction _linkProfileAction;
        private readonly IOpenIdEventSource _openidEventSource;

        public AddUserOperation(
            IResourceOwnerRepository resourceOwnerRepository,
            IClaimRepository claimRepository,
            IAccessTokenStore tokenStore,            
            IScimClientFactory scimClientFactory,        
            ILinkProfileAction linkProfileAction,
            IEnumerable<IAccountFilter> accountFilters,
            IOpenIdEventSource openIdEventSource)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _claimRepository = claimRepository;
            _tokenStore = tokenStore;
            _scimClientFactory = scimClientFactory;
            _accountFilters = accountFilters;
            _linkProfileAction = linkProfileAction;
            _openidEventSource = openIdEventSource;
        }

        public async Task<bool> Execute(AddUserParameter addUserParameter, AuthenticationParameter authenticationParameter, string scimBaseUrl = null, bool addScimResource = false, string issuer = null)
        {
            if (addUserParameter == null)
            {
                throw new ArgumentNullException(nameof(addUserParameter));
            }

            if (string.IsNullOrEmpty(addUserParameter.Login))
            {
                throw new ArgumentNullException(nameof(addUserParameter.Login));
            }

            if (string.IsNullOrWhiteSpace(addUserParameter.Password))
            {
                throw new ArgumentNullException(nameof(addUserParameter.Password));
            }

            if (addScimResource && authenticationParameter == null)
            {
                throw new ArgumentNullException(nameof(authenticationParameter));
            }

            if (addScimResource && string.IsNullOrWhiteSpace(scimBaseUrl))
            {
                throw new ArgumentNullException(nameof(scimBaseUrl));
            }

            // 1. Check the resource owner already exists.
            if (await _resourceOwnerRepository.GetAsync(addUserParameter.Login) != null)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
            }

            var newClaims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, addUserParameter.Login)
            };

            // 2. Populate the claims.
            var existedClaims = await _claimRepository.GetAllAsync();
            if (addUserParameter.Claims != null)
            {
                foreach (var claim in addUserParameter.Claims)
                {
                    if (!newClaims.Any(nc => nc.Type == claim.Type) && existedClaims.Any(c => c.Code == claim.Type))
                    {
                        newClaims.Add(claim);
                    }
                }
            }

            if (_accountFilters != null)
            {
                var isFilterValid = true;
                foreach(var resourceOwnerFilter in _accountFilters)
                {
                    var userFilterResult = await resourceOwnerFilter.Check(newClaims);
                    if (!userFilterResult.IsValid)
                    {
                        isFilterValid = false;
                        foreach(var ruleResult in userFilterResult.AccountFilterRules)
                        {
                            if (!ruleResult.IsValid)
                            {
                                _openidEventSource.Failure($"the filter rule '{ruleResult.RuleName}' failed");
                                foreach (var errorMessage in ruleResult.ErrorMessages)
                                {
                                    _openidEventSource.Failure(errorMessage);
                                }
                            }
                        }
                    }
                }

                if (!isFilterValid)
                {
                    throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheUserIsNotAuthorized);
                }
            }

            // CLAIMS + REQUEST
            // CHECK THE USER CAN BE CREATED.

            // 3. Add the scim resource.
            if (addScimResource)
            {
                var scimResource = await AddScimResource(authenticationParameter, scimBaseUrl, addUserParameter.Login);
                var scimUrl = newClaims.FirstOrDefault(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.ScimId);
                var scimLocation = newClaims.FirstOrDefault(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation);
                if (scimUrl != null)
                {
                    newClaims.Remove(scimUrl);
                }

                if (scimLocation != null)
                {
                    newClaims.Remove(scimLocation);
                }

                newClaims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.ScimId, scimResource.Id));
                newClaims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.ScimLocation, scimResource.Url));
            }

            // 4. Add the resource owner.
            var newResourceOwner = new ResourceOwner
            {
                Id = addUserParameter.Login,
                Claims = newClaims,
                TwoFactorAuthentication = string.Empty,
                IsLocalAccount = true,
                Password = PasswordHelper.ComputeHash(addUserParameter.Password)
            };                        
            if (!await _resourceOwnerRepository.InsertAsync(newResourceOwner))
            {
                throw new IdentityServerException(Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheResourceOwnerCannotBeAdded);
            }

            // 5. Link to a profile.
            if (!string.IsNullOrWhiteSpace(issuer))
            {
                await _linkProfileAction.Execute(addUserParameter.Login, addUserParameter.Login, issuer);
            }

            _openidEventSource.AddResourceOwner(newResourceOwner.Id);
            return true;
        }

        /// <summary>
        /// Create the scim resource and the scim identifier.
        /// </summary>
        /// <param name="subject"></param>
        /// <returns></returns>
        private async Task<ScimUser> AddScimResource(AuthenticationParameter scimOpts, string scimBaseUrl, string subject)
        {
            var grantedToken = await _tokenStore.GetToken(scimOpts.WellKnownAuthorizationUrl, scimOpts.ClientId, scimOpts.ClientSecret, new[]
            {
                "scim_manage"
            });

            var scimResponse = await _scimClientFactory.GetUserClient().AddUser(scimBaseUrl, grantedToken.AccessToken)
                .SetCommonAttributes(subject)
                .Execute();
            var scimId = scimResponse.Content["id"].ToString();
            return new ScimUser(scimId, $"{scimBaseUrl}/Users/{scimId}");
        }
    }
}