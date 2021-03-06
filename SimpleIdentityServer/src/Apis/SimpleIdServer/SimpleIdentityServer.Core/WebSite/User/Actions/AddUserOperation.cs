﻿#region copyright
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

using SimpleIdentityServer.AccountFilter;
using SimpleIdentityServer.Core.Api.Profile.Actions;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.OpenId.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IAddUserOperation
    {
        Task<string> Execute(AddUserParameter addUserParameter, string issuer = null);
    }
    
    public class AddUserOperation : IAddUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IAccountFilter _accountFilter;
        private readonly ILinkProfileAction _linkProfileAction;
        private readonly IOpenIdEventSource _openidEventSource;
        private readonly IEnumerable<IUserClaimsEnricher> _userClaimsEnricherLst;
        private readonly ISubjectBuilder _subjectBuilder;

        public AddUserOperation(IResourceOwnerRepository resourceOwnerRepository, 
            IClaimRepository claimRepository,
            ILinkProfileAction linkProfileAction,
            IAccountFilter accountFilter, 
            IOpenIdEventSource openIdEventSource,
            IEnumerable<IUserClaimsEnricher> userClaimsEnricherLst,
            ISubjectBuilder subjectBuilder)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _claimRepository = claimRepository;
            _linkProfileAction = linkProfileAction;
            _accountFilter = accountFilter;
            _openidEventSource = openIdEventSource;
            _userClaimsEnricherLst = userClaimsEnricherLst;
            _subjectBuilder = subjectBuilder;
        }

        public async Task<string> Execute(AddUserParameter addUserParameter, string issuer = null)
        {
            if (addUserParameter == null)
            {
                throw new ArgumentNullException(nameof(addUserParameter));
            }

            if (string.IsNullOrWhiteSpace(addUserParameter.Password))
            {
                throw new ArgumentNullException(nameof(addUserParameter.Password));
            }
            
            var subject = await _subjectBuilder.BuildSubject().ConfigureAwait(false);
            // 1. Check the resource owner already exists.
            if (await _resourceOwnerRepository.GetAsync(subject) != null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.UnhandledExceptionCode, Errors.ErrorDescriptions.TheRoWithCredentialsAlreadyExists);
            }

            var newClaims = new List<Claim>
            {
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()),
                new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };

            // 2. Populate the claims.
            var existedClaims = await _claimRepository.GetAllAsync().ConfigureAwait(false);
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
            
            var isFilterValid = true;
            var userFilterResult = await _accountFilter.Check(newClaims).ConfigureAwait(false);
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

            if (!isFilterValid)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheUserIsNotAuthorized);
            }

            // 3. Add the scim resource.
            if (_userClaimsEnricherLst != null)
            {
                foreach(var userClaimsEnricher in _userClaimsEnricherLst)
                {
                    await userClaimsEnricher.Enrich(newClaims).ConfigureAwait(false);
                }
            }

            // 4. Add the resource owner.
            var newResourceOwner = new ResourceOwner
            {
                Id = subject,
                Claims = newClaims,
                TwoFactorAuthentication = string.Empty,
                IsLocalAccount = true,
                Password = PasswordHelper.ComputeHash(addUserParameter.Password)
            };                        
            if (!await _resourceOwnerRepository.InsertAsync(newResourceOwner))
            {
                throw new IdentityServerException(Errors.ErrorCodes.UnhandledExceptionCode, Errors.ErrorDescriptions.TheResourceOwnerCannotBeAdded);
            }

            // 5. Link to a profile.
            if (!string.IsNullOrWhiteSpace(issuer))
            {
                await _linkProfileAction.Execute(subject, addUserParameter.ExternalLogin, issuer).ConfigureAwait(false);
            }

            _openidEventSource.AddResourceOwner(newResourceOwner.Id);
            return subject;
        }
    }
}