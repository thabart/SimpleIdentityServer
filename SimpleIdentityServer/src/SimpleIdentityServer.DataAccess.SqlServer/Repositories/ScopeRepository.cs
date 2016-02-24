using System;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using Domains = SimpleIdentityServer.Core.Models;
using Microsoft.Data.Entity;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ScopeRepository : IScopeRepository
    {
         private readonly SimpleIdentityServerContext _context;
        
        public ScopeRepository(SimpleIdentityServerContext context) {
            _context = context;
        }
        
        public bool InsertScope(Domains.Scope scope)
        {
                using (var transaction = _context.Database.BeginTransaction())
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

                        _context.Scopes.Add(record);
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

        public Domains.Scope GetScopeByName(string name)
        {
                var result = _context.Scopes
                    .Include(s => s.Claims)
                    .FirstOrDefault(s => s.Name == name);
                if (result == null)
                {
                    return null;
                }
                
                return result.ToDomain();
        }

        public IList<Domains.Scope> GetAllScopes()
        {
                var result = _context.Scopes.ToList();
                return result.Select(r => r.ToDomain()).ToList();
        }
    }
}
