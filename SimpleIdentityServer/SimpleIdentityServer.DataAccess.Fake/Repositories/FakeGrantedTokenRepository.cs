using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Jwt;
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

        public GrantedToken GetToken(
            string scopes, 
            string clientId, 
            Core.Jwt.JwsPayload idTokenJwsPayload, 
            Core.Jwt.JwsPayload userInfoJwsPayload)
        {
            var grantedTokens = FakeDataSource.Instance()
                .GrantedTokens.Where(g => g.Scope == scopes && g.ClientId == clientId);
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
            if (firstJwsPayload.Count() != secondJwsPayload.Count())
            {
                return false;
            }

            var firstSubject = firstJwsPayload.GetClaimValue(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            var secondSubject =
                secondJwsPayload.GetClaimValue(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject);
            return firstSubject == secondSubject;
        }
    }
}
