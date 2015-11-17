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
            return FakeDataSource.Instance()
                .JsonWebKeys
                .FirstOrDefault(j => j.Kid == kid)
                .ToBusiness();
        }
    }
}
