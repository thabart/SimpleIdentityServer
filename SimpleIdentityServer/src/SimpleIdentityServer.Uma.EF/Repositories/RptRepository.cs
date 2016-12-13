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
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.EF.Extensions;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.EF.Repositories
{
    internal class RptRepository : IRptRepository
    {
        private readonly SimpleIdServerUmaContext _context;

        public RptRepository(SimpleIdServerUmaContext context)
        {
            _context = context;
        }

        public async Task<bool> Insert(Rpt rpt)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    _context.Add(rpt.ToModel());
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<Rpt> Get(string value)
        {
            var record = await _context.Rpts.FirstOrDefaultAsync(r => r.Value == value).ConfigureAwait(false);
            return record == null ? null : record.ToDomain();
        }
    }
}