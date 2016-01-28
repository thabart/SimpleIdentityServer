using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using Domains = SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ScopeRepository : IScopeRepository
    {
        public bool InsertScope(Domains.Scope scope)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var record = new Models.Scope
                        {
                            Name = scope.Name,
                            Description = scope.Description,
                            IsDisplayedInConsent = scope.IsDisplayedInConsent,
                            IsExposed = scope.IsExposed,
                            IsOpenIdScope = scope.IsOpenIdScope,
                            Type = (Models.ScopeType) scope.Type
                        };

                        if (scope.Claims != null &&
                            scope.Claims.Any())
                        {
                            record.Claims = scope.Claims.Select(c => new Models.Claim {Code = c}).ToList();
                        }

                        context.Scopes.Add(record);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            return true;
        }

        public Domains.Scope GetScopeByName(string name)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var result = context.Scopes
                    .Include(s => s.Claims)
                    .FirstOrDefault(s => s.Name == name);
                if (result == null)
                {
                    return null;
                }
                
                return result.ToDomain();
            }
        }

        public IList<Domains.Scope> GetAllScopes()
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var result = context.Scopes.ToList();
                return result.Select(r => r.ToDomain()).ToList();
            }
        }
    }
}
