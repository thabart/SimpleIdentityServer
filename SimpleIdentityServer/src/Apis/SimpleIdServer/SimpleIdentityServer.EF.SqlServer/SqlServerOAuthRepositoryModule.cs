using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.EF.SqlServer
{
    public class SqlServerOAuthRepositoryModule : IModule
    {
        private IDictionary<string, string> _properties;

        public void Init(IDictionary<string, string> properties)
        {
            _properties = properties;
            AspPipelineContext.Instance().ConfigureServiceContext.Initialized += HandleServiceContextInitialized;
        }


        private void HandleServiceContextInitialized(object sender, System.EventArgs e)
        {
            string connectionString;
            if (!_properties.TryGetValue("ConnectionString", out connectionString))
            {
                throw new ModuleException("configuration", "the property 'ConnectionString' is missing");
            }

            AspPipelineContext.Instance().ConfigureServiceContext.Services.AddOAuthSqlServerEF(connectionString);
        }
    }
}