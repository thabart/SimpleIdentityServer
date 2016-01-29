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
        public IList<Jwt.JsonWebKey> GetAll()
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var jsonWebKeys = context.JsonWebKeys.ToList();
                return jsonWebKeys.Select(j => j.ToDomain()).ToList();
            }
        }

        public IList<Jwt.JsonWebKey> GetByAlgorithm(
            Jwt.Use use, 
            Jwt.AllAlg algorithm, 
            Jwt.KeyOperations[] operations)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var algEnum = (Models.AllAlg) algorithm;
                var useEnum = (Models.Use) use;
                var jsonWebKeys = context.JsonWebKeys
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
        }

        public Jwt.JsonWebKey GetByKid(string kid)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var jsonWebKey = context.JsonWebKeys.FirstOrDefault(j => j.Kid == kid);
                if (jsonWebKey == null)
                {
                    return null;
                }

                return jsonWebKey.ToDomain();
            }
        }

        public bool Delete(Jwt.JsonWebKey jsonWebKey)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var jsonWebKeyToBeRemoved = context.JsonWebKeys.FirstOrDefault(j => j.Kid == jsonWebKey.Kid);
                        if (jsonWebKeyToBeRemoved == null)
                        {
                            return false;
                        }

                        context.JsonWebKeys.Remove(jsonWebKeyToBeRemoved);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            return true;
        }

        public bool Insert(Jwt.JsonWebKey jsonWebKey)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                using (var transaction = context.Database.BeginTransaction())
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

                        context.JsonWebKeys.Add(newJsonWebKeyRecord);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            return true;     
        }

        public bool Update(Jwt.JsonWebKey jsonWebKey)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var jsonWebKeyToBeUpdated = context.JsonWebKeys.FirstOrDefault(j => j.Kid == jsonWebKey.Kid);
                        if (jsonWebKeyToBeUpdated == null)
                        {
                            return false;
                        }

                        jsonWebKeyToBeUpdated.SerializedKey = jsonWebKey.SerializedKey;
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
