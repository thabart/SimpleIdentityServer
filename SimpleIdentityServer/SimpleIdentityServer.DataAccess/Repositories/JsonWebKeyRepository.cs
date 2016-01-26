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
    }
}
