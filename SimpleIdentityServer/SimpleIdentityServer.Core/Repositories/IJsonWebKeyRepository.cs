using System.Collections.Generic;

using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IJsonWebKeyRepository
    {
        IList<JsonWebKey> GetAll();

        IList<JsonWebKey> GetByAlgorithm(
            Use use, 
            AllAlg algorithm, 
            KeyOperations[] operations);

        JsonWebKey GetByKid(string kid);
    }
}
