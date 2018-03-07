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

using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;

using Jwt = SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Logging;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class JsonWebKeyRepository : IJsonWebKeyRepository
    {       
        private readonly SimpleIdentityServerContext _context;

        private readonly IManagerEventSource _managerEventSource;

        public JsonWebKeyRepository(SimpleIdentityServerContext context, IManagerEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        public async Task<ICollection<Jwt.JsonWebKey>> GetAllAsync()
        {
            return await _context.JsonWebKeys.Select(s => s.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<ICollection<Jwt.JsonWebKey>> GetByAlgorithmAsync(Jwt.Use use, Jwt.AllAlg algorithm, Jwt.KeyOperations[] operations)
        {
            var algEnum = (Models.AllAlg)algorithm;
            var useEnum = (Models.Use)use;
            var jsonWebKeys = await _context.JsonWebKeys
                .Where(j => j.Use == useEnum && j.Alg == algEnum)
                .ToListAsync()
                .ConfigureAwait(false);
            jsonWebKeys = jsonWebKeys.Where(j =>
            {
                if (string.IsNullOrWhiteSpace(j.KeyOps) || operations == null)
                {
                    return false;
                }

                var keyOperationStrings = j.KeyOps.Split(',');
                var keyOperationEnums = new List<Jwt.KeyOperations>();
                foreach (var keyOperationString in keyOperationStrings)
                {
                    Jwt.KeyOperations keyOperationEnum;
                    if (!Enum.TryParse(keyOperationString, out keyOperationEnum))
                    {
                        continue;
                    }

                    keyOperationEnums.Add(keyOperationEnum);
                }

                return operations.All(o => keyOperationEnums.Contains(o));
            }).ToList();

            return jsonWebKeys.Select(j => j.ToDomain()).ToList();
        }

        public async Task<Jwt.JsonWebKey> GetByKidAsync(string kid)
        {
            var jsonWebKey = await _context.JsonWebKeys.FirstOrDefaultAsync(j => j.Kid == kid).ConfigureAwait(false);
            if (jsonWebKey == null)
            {
                return null;
            }

            return jsonWebKey.ToDomain();
        }

        public async Task<bool> DeleteAsync(Jwt.JsonWebKey jsonWebKey)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var jsonWebKeyToBeRemoved = await _context.JsonWebKeys.FirstOrDefaultAsync(j => j.Kid == jsonWebKey.Kid).ConfigureAwait(false);
                    if (jsonWebKeyToBeRemoved == null)
                    {
                        return false;
                    }

                    _context.JsonWebKeys.Remove(jsonWebKeyToBeRemoved);
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

        public async Task<bool> InsertAsync(Jwt.JsonWebKey jsonWebKey)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var keyOperations = string.Empty;
                    var x5u = string.Empty;
                    if (jsonWebKey.KeyOps != null)
                    {
                        var keyOperationNumbers = jsonWebKey.KeyOps.Select(s => (int)s);
                        keyOperations = string.Join(",", keyOperationNumbers);
                    }

                    if (jsonWebKey.X5u != null)
                    {
                        x5u = jsonWebKey.X5u.AbsoluteUri;
                    }

                    var newJsonWebKeyRecord = new Models.JsonWebKey
                    {
                        Kid = jsonWebKey.Kid,
                        Alg = (Models.AllAlg)jsonWebKey.Alg,
                        Kty = (Models.KeyType)jsonWebKey.Kty,
                        Use = (Models.Use)jsonWebKey.Use,
                        KeyOps = keyOperations,
                        SerializedKey = jsonWebKey.SerializedKey,
                        X5t = jsonWebKey.X5t,
                        X5tS256 = jsonWebKey.X5tS256,
                        X5u = x5u
                    };

                    _context.JsonWebKeys.Add(newJsonWebKeyRecord);
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

        public async Task<bool> UpdateAsync(Jwt.JsonWebKey jsonWebKey)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var jsonWebKeyToBeUpdated = await _context.JsonWebKeys.FirstOrDefaultAsync(j => j.Kid == jsonWebKey.Kid).ConfigureAwait(false);
                    if (jsonWebKeyToBeUpdated == null)
                    {
                        return false;
                    }

                    jsonWebKeyToBeUpdated.SerializedKey = jsonWebKey.SerializedKey;
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
