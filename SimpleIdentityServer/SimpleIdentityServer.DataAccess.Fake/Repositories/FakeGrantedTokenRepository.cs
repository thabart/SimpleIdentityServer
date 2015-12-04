using System.Collections.Generic;
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
                if (CompareDictionaries(idTokenJwsPayload, grantedToken.IdTokenPayLoad) &&
                    CompareDictionaries(userInfoJwsPayload, grantedToken.UserInfoPayLoad))
                {
                    return grantedToken.ToBusiness();
                }
            }

            return null;
        }

        public bool CompareDictionaries<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2)
        {
            if (dict1 == dict2)
            {
                return true;
            }

            if ((dict1 == null) || (dict2 == null))
            {
                return false;
            }

            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            var valueComparer = EqualityComparer<TValue>.Default;

            foreach (var kvp in dict1)
            {
                TValue value2;
                if (!dict2.TryGetValue(kvp.Key, out value2))
                {
                    return false;
                }
                if (!valueComparer.Equals(kvp.Value, value2))
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}
