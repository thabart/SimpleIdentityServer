namespace SimpleIdentityServer.Scim.Host.Configurations
{
    public enum DbTypes
    {
        SQLSERVER,
        POSTGRES,
        INMEMORY
    }

    public enum CachingTypes
    {
        INMEMORY,
        REDIS
    }

    public sealed class DbOptions
    {
        public DbOptions()
        {
            Type = DbTypes.INMEMORY;
        }

        public DbTypes Type { get; set; }
        public string ConnectionString { get; set; }
        public bool IsDataMigrated { get; set; }
    }

    public sealed class CachingOptions
    {
        public CachingOptions()
        {
            Type = CachingTypes.INMEMORY;
            Port = 6379;
        }

        public CachingTypes Type { get; set; }
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public int Port { get; set; }
    }

    public sealed class OAuthOptions
    {
        public string IntrospectionEndpoints { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public sealed class ScimConfiguration
    {
        public ScimConfiguration()
        {
            DataSource = new DbOptions();
            AuthorizationServer = new OAuthOptions();
            Caching = new CachingOptions();
        }

        public OAuthOptions AuthorizationServer { get; set; }
        public DbOptions DataSource { get; set; }
        public CachingOptions Caching { get; set; }
    }
}
