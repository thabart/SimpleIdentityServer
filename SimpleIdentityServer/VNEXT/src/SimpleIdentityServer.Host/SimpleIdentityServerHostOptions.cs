using System.Collections.Generic;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Host 
{
    public enum DataSourceTypes 
    {
        InMemory,
        SqlServer
    }
    
    public class SimpleIdentityServerHostOptions 
    {
        public DataSourceTypes DataSourceType { get; set;}
        
        public List<Client> Clients { get; set; }
        
        public List<JsonWebKey> JsonWebKeys { get; set;}
        
        public List<ResourceOwner> ResourceOwners { get; set;}
        
        public List<Scope> Scopes { get; set; }
        
        public List<Translation> Translations { get; set; }
    }
}