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
        /// <summary>
        /// Choose the type of your DataSource
        /// </summary>
        public DataSourceTypes DataSourceType { get; set;}

        /// <summary>
        /// Enable or disable swagger.
        /// </summary>
        public bool IsSwaggerEnabled { get; set; }

        /// <summary>
        /// If swagger is enabled and the SimpleIdentityServer is hosted under a relative path, then the URL needs to be specified.
        /// Otherwise the information cannot be extracted from the endpoints.
        /// </summary>
        public string SwaggerUrl { get; set; }

        /// <summary>
        /// Enable or disable the developer mode
        /// </summary>
        public bool IsDeveloperModeEnabled { get; set; }
        
        /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString { get; set;}
        
        /// <summary>
        /// List of fake clients
        /// </summary>
        public List<Client> Clients { get; set; }
        
        /// <summary>
        /// List of fake Json Web Keys
        /// </summary>
        public List<JsonWebKey> JsonWebKeys { get; set;}
        
        /// <summary>
        /// List of fake resource owners
        /// </summary>
        public List<ResourceOwner> ResourceOwners { get; set;}
        
        /// <summary>
        /// List of fake scopes
        /// </summary>
        public List<Scope> Scopes { get; set; }
        
        /// <summary>
        /// List of fake translations
        /// </summary>
        public List<Translation> Translations { get; set; }

        /// <summary>
        /// Gets or sets the microsoft client id
        /// </summary>
        public string MicrosoftClientId { get; set; }

        /// <summary>
        /// Gets or sets the microsoft secret
        /// </summary>
        public string MicrosoftSecret { get; set; }
    }
}