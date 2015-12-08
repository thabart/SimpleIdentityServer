using System.Linq;
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
        
        public AuthorizationCode GetAuthorizationCode(string code)
        {
            if (!FakeDataSource.Instance().AuthorizationCodes.Any())
            {
                return null;
            }

            var result = FakeDataSource.Instance().AuthorizationCodes.FirstOrDefault(a => a.Code == code);
            return result == null ? null : result.ToBusiness();
        }
        
        public bool RemoveAuthorizationCode(string code)
        {
            var authCodeToRemove = FakeDataSource.Instance().AuthorizationCodes.FirstOrDefault(a => a.Code == code);
            FakeDataSource.Instance().AuthorizationCodes.Remove(authCodeToRemove);
            return true;
        }
    }
}
