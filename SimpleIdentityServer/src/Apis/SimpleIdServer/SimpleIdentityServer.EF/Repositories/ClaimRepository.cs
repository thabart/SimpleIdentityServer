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

using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Common.Results;
using SimpleIdentityServer.EF.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EF.Repositories
{
    internal class ClaimRepository : IClaimRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly IManagerEventSource _managerEventSource;

        public ClaimRepository(SimpleIdentityServerContext context, IManagerEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<SearchClaimsResult> Search(SearchClaimsParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IQueryable<Models.Claim> claims = _context.Claims;
            if (parameter.ClaimKeys != null)
            {
                claims = claims.Where(c => parameter.ClaimKeys.Any(ck => c.Code.Contains(ck)));
            }

            var nbResult = await claims.CountAsync().ConfigureAwait(false);
            if (parameter.Order != null)
            {
                switch (parameter.Order.Target)
                {
                    case "update_datetime":
                        switch (parameter.Order.Type)
                        {
                            case OrderTypes.Asc:
                                claims = claims.OrderBy(c => c.UpdateDateTime);
                                break;
                            case OrderTypes.Desc:
                                claims = claims.OrderByDescending(c => c.UpdateDateTime);
                                break;
                        }
                        break;
                }
            }
            else
            {
                claims = claims.OrderByDescending(c => c.UpdateDateTime);
            }

            if (parameter.IsPagingEnabled)
            {
                claims = claims.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return new SearchClaimsResult
            {
                Content = await claims.Select(c => c.ToDomain()).ToListAsync().ConfigureAwait(false),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            };
        }
        
        public async Task<IEnumerable<ClaimAggregate>> GetAllAsync()
        {
            return await _context.Claims.Select(c => c.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<ClaimAggregate> GetAsync(string name)
        {
            var result = await _context.Claims.FirstOrDefaultAsync(c => c.Code == name).ConfigureAwait(false);
            return result == null ? null : result.ToDomain();
        }

        public async Task<bool> InsertAsync(AddClaimParameter claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var newClaim = new Models.Claim
                    {
                        Code = claim.Code,
                        IsIdentifier = false, //claim.IsIdentifier,
                        CreateDateTime = DateTime.UtcNow,
                        UpdateDateTime = DateTime.UtcNow
                    };

                    _context.Claims.Add(newClaim);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> Delete(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var connectedClaim = await _context.Claims.FirstOrDefaultAsync(c => c.Code == code).ConfigureAwait(false);
                    if (connectedClaim == null)
                    {
                        return false;
                    }

                    _context.Claims.Remove(connectedClaim);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> Update(ClaimAggregate claim)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var connectedClaim = await _context.Claims.FirstOrDefaultAsync(c => c.Code == claim.Code).ConfigureAwait(false);
                    if (connectedClaim == null)
                    {
                        return false;
                    }

                    connectedClaim.IsIdentifier = claim.IsIdentifier;
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }
    }
}
