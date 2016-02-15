using System.Linq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeAuthorizationCodeRepository : IAuthorizationCodeRepository
    {
        private readonly FakeDataSource _fakeDataSource;
        
        public FakeAuthorizationCodeRepository(FakeDataSource fakeDataSource) 
        {
            _fakeDataSource = fakeDataSource;
        }
        
        public bool AddAuthorizationCode(AuthorizationCode authorizationCode)
        {
            _fakeDataSource.AuthorizationCodes.Add(authorizationCode.ToFake());
            return true;
        }
        
        public AuthorizationCode GetAuthorizationCode(string code)
        {
            if (!_fakeDataSource.AuthorizationCodes.Any())
            {
                return null;
            }

            var result = _fakeDataSource.AuthorizationCodes.FirstOrDefault(a => a.Code == code);
            return result == null ? null : result.ToBusiness();
        }
        
        public bool RemoveAuthorizationCode(string code)
        {
            var authCodeToRemove = _fakeDataSource.AuthorizationCodes.FirstOrDefault(a => a.Code == code);
            _fakeDataSource.AuthorizationCodes.Remove(authCodeToRemove);
            return true;
        }
    }
}
