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

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Linq;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
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

        public ConfirmationCode Get(string code)
        {
            var confirmationCode = _context.ConfirmationCodes.FirstOrDefault(c => c.Code == code);
            if (confirmationCode == null)
            {
                return null;
            }

            return confirmationCode.ToDomain();
        }

        public bool AddCode(ConfirmationCode code)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var record = code.ToModel();
                    _context.ConfirmationCodes.Add(record);
                    _context.SaveChanges();
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

        public bool Remove(string code)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var confirmationCode = _context.ConfirmationCodes.FirstOrDefault(c => c.Code == code);
                    if (confirmationCode == null)
                    {
                        return false;
                    }

                    _context.ConfirmationCodes.Remove(confirmationCode);
                    _context.SaveChanges();
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
