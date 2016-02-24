using System;
using System.Linq;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Models;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed  class AuthorizationCodeRepository : IAuthorizationCodeRepository
    {
        private readonly SimpleIdentityServerContext _context;
        
        #region Constructor
        
        public AuthorizationCodeRepository(SimpleIdentityServerContext context) 
        {
            _context = context;
        }
        
        #endregion
        
        #region Public methods
        
        public bool AddAuthorizationCode(Core.Models.AuthorizationCode authorizationCode)
        {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var newAuthorizationCode = new AuthorizationCode
                        {
                            Code = authorizationCode.Code,
                            ClientId = authorizationCode.ClientId,
                            CreateDateTime = authorizationCode.CreateDateTime,
                            Scopes = authorizationCode.Scopes,
                            RedirectUri = authorizationCode.RedirectUri,
                            IdTokenPayload = authorizationCode.IdTokenPayload ==  null ? string.Empty : authorizationCode.IdTokenPayload.SerializeWithJavascript(),
                            UserInfoPayLoad = authorizationCode.UserInfoPayLoad == null ? string.Empty : authorizationCode.UserInfoPayLoad.SerializeWithJavascript()
                        };
                        _context.AuthorizationCodes.Add(newAuthorizationCode);
                        _context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }

            return true;
        }

        public Core.Models.AuthorizationCode GetAuthorizationCode(string code)
        {
                var authorizationCode = _context.AuthorizationCodes.FirstOrDefault(a => a.Code == code);
                if (authorizationCode == null)
                {
                    return null;
                }

                return authorizationCode.ToDomain();
        }

        public bool RemoveAuthorizationCode(string code)
        {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var authorizationCode = _context.AuthorizationCodes.FirstOrDefault(a => a.Code == code);
                        if (authorizationCode == null)
                        {
                            return false;
                        }

                        _context.AuthorizationCodes.Remove(authorizationCode);
                        _context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }

            return true;
        }
        
        #endregion
    }
}
