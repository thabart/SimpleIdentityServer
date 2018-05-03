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
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.EF.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EF.Repositories
{
    public class ConfirmationCodeRepository : IConfirmationCodeRepository
    {
        private readonly SimpleIdentityServerContext _context;

        private readonly IManagerEventSource _managerEventSource;

        public ConfirmationCodeRepository(SimpleIdentityServerContext context, IManagerEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<ConfirmationCode> GetAsync(string code)
        {
            var confirmationCode = await _context.ConfirmationCodes.FirstOrDefaultAsync(c => c.Code == code).ConfigureAwait(false);
            if (confirmationCode == null)
            {
                return null;
            }

            return confirmationCode.ToDomain();
        }

        public async Task<bool> AddAsync(ConfirmationCode code)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var record = code.ToModel();
                    _context.ConfirmationCodes.Add(record);
                    await _context.SaveChangesAsync().ConfigureAwait(false); ;
                    transaction.Commit();
                    return true;
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    _managerEventSource.Failure(ex);
                    return false;
                }
            }
        }

        public async Task<bool> RemoveAsync(string code)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var confirmationCode = await _context.ConfirmationCodes.FirstOrDefaultAsync(c => c.Code == code).ConfigureAwait(false); ;
                    if (confirmationCode == null)
                    {
                        return false;
                    }

                    _context.ConfirmationCodes.Remove(confirmationCode);
                    await _context.SaveChangesAsync().ConfigureAwait(false); ;
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    _managerEventSource.Failure(ex);
                    return false;
                }
            }
        }
    }
}
