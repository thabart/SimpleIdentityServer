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
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed  class AuthorizationCodeRepository : IAuthorizationCodeRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly IManagerEventSource _managerEventSource;
        
        public AuthorizationCodeRepository(
            SimpleIdentityServerContext context,
            IManagerEventSource managerEventSource) 
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<bool> AddAsync(Core.Models.AuthorizationCode authorizationCode)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var newAuthorizationCode = new Models.AuthorizationCode
                    {
                        Code = authorizationCode.Code,
                        ClientId = authorizationCode.ClientId,
                        CreateDateTime = authorizationCode.CreateDateTime,
                        Scopes = authorizationCode.Scopes,
                        RedirectUri = authorizationCode.RedirectUri,
                        IdTokenPayload = authorizationCode.IdTokenPayload == null ? string.Empty : authorizationCode.IdTokenPayload.SerializeWithJavascript(),
                        UserInfoPayLoad = authorizationCode.UserInfoPayLoad == null ? string.Empty : authorizationCode.UserInfoPayLoad.SerializeWithJavascript()
                    };
                    _context.AuthorizationCodes.Add(newAuthorizationCode);
                    await _context.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                }
            }

            return true;
        }
        
        public async Task<Core.Models.AuthorizationCode> GetAsync(string code)
        {
            var authorizationCode = await _context.AuthorizationCodes
                .FirstOrDefaultAsync(a => a.Code == code)
                .ConfigureAwait(false);
            if (authorizationCode == null)
            {
                return null;
            }

            return authorizationCode.ToDomain();
        }

        public async Task<bool> RemoveAsync(string code)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead).ConfigureAwait(false))
            {
                try
                {
                    var authorizationCode = await _context.AuthorizationCodes.FirstOrDefaultAsync(a => a.Code == code).ConfigureAwait(false);
                    if (authorizationCode == null)
                    {
                        return false;
                    }

                    _context.AuthorizationCodes.Remove(authorizationCode);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                }
            }

            return true;
        }
    }
}
