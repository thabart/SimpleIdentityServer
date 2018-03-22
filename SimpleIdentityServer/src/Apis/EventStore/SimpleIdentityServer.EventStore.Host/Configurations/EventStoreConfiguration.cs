namespace SimpleIdentityServer.EventStore.Host.Configurations
{
    public enum DbTypes
    {
        INMEMORY,
        SQLITE,
        POSTGRES,
        SQLSERVER
    }

    public class DataSourceConfiguration
    {
        public DataSourceConfiguration()
        {
            IsDataMigrated = false;
        }

        public bool IsDataMigrated { get; set; }
        public DbTypes Type { get; set; }
        public string ConnectionString { get; set; }
    }

    public class EventStoreConfiguration
    {
        public EventStoreConfiguration()
        {
            DataSource = new DataSourceConfiguration();
        }

        public DataSourceConfiguration DataSource { get; set; }
    }
}
