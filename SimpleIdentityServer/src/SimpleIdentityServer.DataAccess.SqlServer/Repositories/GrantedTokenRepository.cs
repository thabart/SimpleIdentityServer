using System;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using System.Linq;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public class GrantedTokenRepository : IGrantedTokenRepository
    {
        private readonly SimpleIdentityServerContext _context;
        
        public GrantedTokenRepository(SimpleIdentityServerContext context) {
            _context = context;
        }
        
        #region Public methods

        public GrantedToken GetToken(string accessToken)
        {
                var grantedToken = _context.GrantedTokens.FirstOrDefault(g => g.AccessToken == accessToken);
                if (grantedToken == null)
                {
                    return null;
                }

                return grantedToken.ToDomain();
        }

        public GrantedToken GetTokenByRefreshToken(string refreshToken)
        {
                var grantedToken = _context.GrantedTokens.FirstOrDefault(g => g.RefreshToken == refreshToken);
                if (grantedToken == null)
                {
                    return null;
                }

                return grantedToken.ToDomain();
        }

        public GrantedToken GetToken(
            string scopes, 
            string clientId, 
            JwsPayload idTokenJwsPayload, 
            JwsPayload userInfoJwsPayload)
        {
                var grantedTokens = _context.GrantedTokens.Where(g => g.Scope == scopes && g.ClientId == clientId)
                    .ToList();
                if (grantedTokens == null || !grantedTokens.Any())
                {
                    return null;
                }

                foreach (var grantedToken in grantedTokens)
                {
                    var grantedTokenIdTokenJwsPayload = grantedToken.IdTokenPayLoad.DeserializeWithJavascript<JwsPayload>();
                    var grantedTokenUserInfoJwsPayload = grantedToken.UserInfoPayLoad.DeserializeWithJavascript<JwsPayload>();
                    if (CompareJwsPayload(idTokenJwsPayload, grantedTokenIdTokenJwsPayload) &&
                        CompareJwsPayload(userInfoJwsPayload, grantedTokenUserInfoJwsPayload))
                    {
                        return grantedToken.ToDomain();
                    }
                }

            return null;
        }

        public bool Insert(GrantedToken grantedToken)
        {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var record = new Models.GrantedToken
                        {
                            AccessToken = grantedToken.AccessToken,
                            ClientId = grantedToken.ClientId,
                            CreateDateTime = grantedToken.CreateDateTime,
                            ExpiresIn = grantedToken.ExpiresIn,
                            RefreshToken = grantedToken.RefreshToken,
                            Scope = grantedToken.Scope,
                            IdTokenPayLoad = grantedToken.IdTokenPayLoad == null ? string.Empty : grantedToken.IdTokenPayLoad.SerializeWithJavascript(),
                            UserInfoPayLoad = grantedToken.UserInfoPayLoad == null ? string.Empty : grantedToken.UserInfoPayLoad.SerializeWithJavascript()
                        };

                        _context.GrantedTokens.Add(record);
                        _context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

            return true;
        }

        public bool Delete(GrantedToken grantedToken)
        {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var token = _context.GrantedTokens.FirstOrDefault(g => g.AccessToken == grantedToken.AccessToken);
                        _context.GrantedTokens.Remove(token);
                        _context.SaveChanges();
                        transaction.Commit();
                    }
                    catch(Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

            return true;
        }

        #endregion

        #region Private static methods

        private static bool CompareJwsPayload(JwsPayload firstJwsPayload, JwsPayload secondJwsPayload)
        {
            foreach (var record in firstJwsPayload)
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

        #endregion
    }
}
