using System.Collections.Generic;
using SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Configuration
{
    public static class Scopes
    {
        public static List<Scope> Get()
        {
            return new List<Scope>
            {
                new Scope
                {
                    Name = "BlogApi"
                },
                new Scope
                {
                    Name = "BlogApi:AddArticle"
                }
            };
        } 
    }
}