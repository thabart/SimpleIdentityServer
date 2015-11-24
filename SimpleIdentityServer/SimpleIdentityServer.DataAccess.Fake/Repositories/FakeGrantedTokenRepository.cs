using System.Linq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeGrantedTokenRepository : IGrantedTokenRepository
    {
        public bool Insert(GrantedToken grantedToken)
        {
            FakeDataSource.Instance().GrantedTokens.Add(grantedToken.ToFake());
            return true;
        }

        public GrantedToken GetToken(string accessToken)
        {
            var result = FakeDataSource.Instance().GrantedTokens.FirstOrDefault(g => g.AccessToken == accessToken);
            return result == null ? null : result.ToBusiness();
        }
    }
}
