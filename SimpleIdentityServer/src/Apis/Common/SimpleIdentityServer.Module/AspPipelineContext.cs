using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimpleIdentityServer.Module
{
    public class AspPipelineContext
    {
        private ConfigureServiceContext _configureServiceContext;
        private ApplicationBuilderContext _applicationBuilderContext;
        private static AspPipelineContext _instance;

        private AspPipelineContext()
        {
            _configureServiceContext = new ConfigureServiceContext();
            _applicationBuilderContext = new ApplicationBuilderContext();
        }

        public static AspPipelineContext Instance()
        {
            if (_instance == null)
            {
                _instance = new AspPipelineContext();
            }

            return _instance;
        }

        #region Public methods

        public void StartConfigureServices(IServiceCollection services)
        {
            _configureServiceContext.Init(services);
        }

        public void StartConfigureApplicationBuilder(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _applicationBuilderContext.Init(app, env, loggerFactory);
        }

        #endregion

        #region Properties

        public ConfigureServiceContext ConfigureServiceContext
        {
            get
            {
                return _configureServiceContext;
            }
        }

        public ApplicationBuilderContext ApplicationBuilderContext
        {
            get
            {
                return _applicationBuilderContext;
            }
        }

        #endregion
    }
}
