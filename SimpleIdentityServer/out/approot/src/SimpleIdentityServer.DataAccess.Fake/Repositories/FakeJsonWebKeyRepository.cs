using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeJsonWebKeyRepository : IJsonWebKeyRepository
    {
        private readonly FakeDataSource _fakeDataSource;
        
        public FakeJsonWebKeyRepository(FakeDataSource fakeDataSource) 
        {
            _fakeDataSource = fakeDataSource;
        }
        
        public IList<JsonWebKey> GetAll()
        {
            return _fakeDataSource.JsonWebKeys.Select(wk => wk.ToBusiness()).ToList();
        }

        public IList<JsonWebKey> GetByAlgorithm(
            Use use, 
            AllAlg algorithm,
            KeyOperations[] keyOperations)
        {
           return _fakeDataSource
                    .JsonWebKeys.Where(wk => wk.Use == use.ToFake() && wk.Alg == algorithm.ToFake() && keyOperations.All(ko => wk.KeyOps.Contains(ko.ToFake())))
                    .Select(j => j.ToBusiness())
                    .ToList();
        }

        public JsonWebKey GetByKid(string kid)
        {
            var result = _fakeDataSource
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
            _fakeDataSource.JsonWebKeys.Add(fakeJsonWebKey);
            return true;
        }
        public bool Delete(JsonWebKey jsonWebKey)
        {
            var jsonWebKeyToBeRemoved = _fakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == jsonWebKey.Kid);
            if (jsonWebKeyToBeRemoved == null)
            {
                return false;
            }

            _fakeDataSource.JsonWebKeys.Remove(jsonWebKeyToBeRemoved);
            return true;
        }

        public bool Update(JsonWebKey jsonWebKey)
        {
            var jsonWebKeyToBeUpdated = _fakeDataSource.JsonWebKeys.FirstOrDefault(j => j.Kid == jsonWebKey.Kid);
            if (jsonWebKeyToBeUpdated == null)
            {
                return false;
            }

            jsonWebKeyToBeUpdated.SerializedKey = jsonWebKey.SerializedKey;
            return true;
        }
    }
}
