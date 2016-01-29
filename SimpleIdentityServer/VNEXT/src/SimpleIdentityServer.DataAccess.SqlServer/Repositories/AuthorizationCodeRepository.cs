using System;
using System.Linq;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed  class AuthorizationCodeRepository : IAuthorizationCodeRepository
    {
        public bool AddAuthorizationCode(Core.Models.AuthorizationCode authorizationCode)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                using (var transaction = context.Database.BeginTransaction())
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
                        context.AuthorizationCodes.Add(newAuthorizationCode);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
            }

            return true;
        }

        public Core.Models.AuthorizationCode GetAuthorizationCode(string code)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var authorizationCode = context.AuthorizationCodes.FirstOrDefault(a => a.Code == code);
                if (authorizationCode == null)
                {
                    return null;
                }

                return authorizationCode.ToDomain();
            }
        }

        public bool RemoveAuthorizationCode(string code)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var authorizationCode = context.AuthorizationCodes.FirstOrDefault(a => a.Code == code);
                        if (authorizationCode == null)
                        {
                            return false;
                        }

                        context.AuthorizationCodes.Remove(authorizationCode);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
            }

            return true;
        }
    }
}
