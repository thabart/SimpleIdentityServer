using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

using MODELS = SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeJsonWebKeyRepository : IJsonWebKeyRepository
    {
        public IList<JsonWebKey> GetAll()
        {
            return FakeDataSource.Instance().JsonWebKeys.Select(wk => wk.ToBusiness()).ToList();
        }

        public IList<JsonWebKey> GetByAlgorithm(
            Use use, 
            AllAlg algorithm,
            KeyOperations[] keyOperations)
        {
           return FakeDataSource.Instance()
                    .JsonWebKeys.Where(wk => wk.Use == use.ToFake() && wk.Alg == algorithm.ToFake() && keyOperations.All(ko => wk.KeyOps.Contains(ko.ToFake())))
                    .Select(j => j.ToBusiness())
                    .ToList();
        }

        public JsonWebKey GetByKid(string kid)
        {
            var result = FakeDataSource.Instance()
                .JsonWebKeys
                .FirstOrDefault(j => j.Kid == kid);
            if (result == null)
            {
                return null;
            }

            return result.ToBusiness();
        }

        public bool Insert(JsonWebKey jsonWebKey)
        {
            var fakeJsonWebKey = jsonWebKey.ToFake();
            FakeDataSource.Instance().JsonWebKeys.Add(fakeJsonWebKey);
            return true;
        }
        public bool Delete(JsonWebKey jsonWebKey)
        {
            var jsonWebKeyToBeRemoved = FakeDataSource.Instance().JsonWebKeys.FirstOrDefault(j => j.Kid == jsonWebKey.Kid);
            if (jsonWebKeyToBeRemoved == null)
            {
                return false;
            }

            FakeDataSource.Instance().JsonWebKeys.Remove(jsonWebKeyToBeRemoved);
            return true;
        }
    }
}
