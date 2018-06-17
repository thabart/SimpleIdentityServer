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

using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IUpdateUserClaimsOperation
    {
        Task<bool> Execute(string subject, IEnumerable<Claim> claims);
    }

    internal class UpdateUserClaimsOperation : IUpdateUserClaimsOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IClaimRepository _claimRepository;
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;

        public UpdateUserClaimsOperation(
            IResourceOwnerRepository resourceOwnerRepository,
            IClaimRepository claimRepository,
            IAuthenticateResourceOwnerService authenticateResourceOwnerService)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _claimRepository = claimRepository;
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
        }
        
        public async Task<bool> Execute(string subject, IEnumerable<Claim> claims)
        {
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            var resourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(subject);
            if (resourceOwner == null)
            {
                throw new IdentityServerException(Errors.ErrorCodes.InternalError, Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            var supportedClaims = await _claimRepository.GetAllAsync();
            claims = claims.Where(c => supportedClaims.Any(sp => sp.Code == c.Type && !Jwt.Constants.NotEditableResourceOwnerClaimNames.Contains(c.Type)));
            var claimsToBeRemoved = resourceOwner.Claims
                .Where(cl => claims.Any(c => c.Type == cl.Type))
                .Select(cl => resourceOwner.Claims.IndexOf(cl))
                .OrderByDescending(p => p)
                .ToList();
            foreach(var index in claimsToBeRemoved)
            {
                resourceOwner.Claims.RemoveAt(index);
            }         
            
            foreach(var claim in claims)
            {
                resourceOwner.Claims.Add(claim);
            }

            Claim updatedClaim;
            if (((updatedClaim = resourceOwner.Claims.FirstOrDefault(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt)) != null))
            {
                resourceOwner.Claims.Remove(updatedClaim);
            }

            resourceOwner.Claims.Add(new Claim(Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, DateTime.UtcNow.ToString()));
            return await _resourceOwnerRepository.UpdateAsync(resourceOwner);
        }
    }
}
