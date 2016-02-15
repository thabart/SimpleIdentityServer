using System;
using System.Linq;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.Fake.Extensions;

namespace SimpleIdentityServer.DataAccess.Fake.Repositories
{
    public class FakeGrantedTokenRepository : IGrantedTokenRepository
    {
        private readonly FakeDataSource _fakeDataSource;
        
        public FakeGrantedTokenRepository(FakeDataSource fakeDataSource) 
        {
            _fakeDataSource = fakeDataSource;
        }
        
        
        public bool Insert(GrantedToken grantedToken)
        {
            _fakeDataSource.GrantedTokens.Add(grantedToken.ToFake());
            return true;
        }

        public GrantedToken GetToken(string accessToken)
        {
            var result = _fakeDataSource.GrantedTokens.FirstOrDefault(g => g.AccessToken == accessToken);
            return result == null ? null : result.ToBusiness();
        }

        public GrantedToken GetToken(
            string scopes, 
            string clientId, 
            Core.Jwt.JwsPayload idTokenJwsPayload, 
            Core.Jwt.JwsPayload userInfoJwsPayload)
        {
            var grantedTokens = _fakeDataSource.GrantedTokens.Where(g => g.Scope == scopes && g.ClientId == clientId);
            if (grantedTokens == null || !grantedTokens.Any())
            {
                return null;
            }

            foreach (var grantedToken in grantedTokens)
            {
                if (CompareJwsPayload(idTokenJwsPayload, grantedToken.IdTokenPayLoad) &&
                    CompareJwsPayload(userInfoJwsPayload, grantedToken.UserInfoPayLoad))
                {
                    return grantedToken.ToBusiness();
                }
            }

            return null;
        }

        public bool CompareJwsPayload(JwsPayload firstJwsPayload, JwsPayload secondJwsPayload)
        {
            foreach(var record in firstJwsPayload)
            {
                if (record.Key == Constants.StandardClaimNames.CHash || 
                    record.Key == Constants.StandardClaimNames.AtHash)
                {
                    continue;
                }

                if (!secondJwsPayload.ContainsKey(record.Key))
                {
                    return false;
                }

                if (secondJwsPayload[record.Key] != record.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public GrantedToken GetTokenByRefreshToken(string refreshToken)
        {
            var result = _fakeDataSource.GrantedTokens.FirstOrDefault(g => g.RefreshToken == refreshToken);
            return result == null ? null : result.ToBusiness();
        }

        public bool Delete(GrantedToken grantedToken)
        {
            var grantedTokenToBeRemoved = _fakeDataSource.GrantedTokens.FirstOrDefault(c => c.AccessToken == grantedToken.AccessToken);
            if (grantedTokenToBeRemoved == null)
            {
                return false;
            }

            _fakeDataSource.GrantedTokens.Remove(grantedTokenToBeRemoved);
            return true;
        }
    }
}
