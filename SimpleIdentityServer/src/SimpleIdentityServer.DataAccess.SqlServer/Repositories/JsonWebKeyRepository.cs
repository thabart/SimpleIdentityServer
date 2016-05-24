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

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class JsonWebKeyRepository : IJsonWebKeyRepository
    {       
         private readonly SimpleIdentityServerContext _context;
        
        public JsonWebKeyRepository(SimpleIdentityServerContext context) {
            _context = context;
        }
        
        public IList<Jwt.JsonWebKey> GetAll()
        {
                var jsonWebKeys = _context.JsonWebKeys.ToList();
                return jsonWebKeys.Select(j => j.ToDomain()).ToList();
        }

        public IList<Jwt.JsonWebKey> GetByAlgorithm(
            Jwt.Use use,
            Jwt.AllAlg algorithm,
            Jwt.KeyOperations[] operations)
        {
            var algEnum = (Models.AllAlg)algorithm;
            var useEnum = (Models.Use)use;
            var jsonWebKeys = _context.JsonWebKeys
                .Where(j => j.Use == useEnum && j.Alg == algEnum)
                .ToList();
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

        public Jwt.JsonWebKey GetByKid(string kid)
        {
            var jsonWebKey = _context.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
            if (jsonWebKey == null)
            {
                return null;
            }

            return jsonWebKey.ToDomain();
        }

        public bool Delete(Jwt.JsonWebKey jsonWebKey)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var jsonWebKeyToBeRemoved = _context.JsonWebKeys.FirstOrDefault(j => j.Kid == jsonWebKey.Kid);
                    if (jsonWebKeyToBeRemoved == null)
                    {
                        return false;
                    }

                    _context.JsonWebKeys.Remove(jsonWebKeyToBeRemoved);
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public bool Insert(Jwt.JsonWebKey jsonWebKey)
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
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public bool Update(Jwt.JsonWebKey jsonWebKey)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var jsonWebKeyToBeUpdated = _context.JsonWebKeys.FirstOrDefault(j => j.Kid == jsonWebKey.Kid);
                    if (jsonWebKeyToBeUpdated == null)
                    {
                        return false;
                    }

                    jsonWebKeyToBeUpdated.SerializedKey = jsonWebKey.SerializedKey;
                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }
    }
}
