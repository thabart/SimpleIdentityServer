using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeAuthorizationCodeRepository : IAuthorizationCodeRepository
    {
        public bool AddAuthorizationCode(AuthorizationCode authorizationCode)
        {
            FakeDataSource.Instance().AuthorizationCodes.Add(authorizationCode.ToFake());
            return true;
        }
    }
}
