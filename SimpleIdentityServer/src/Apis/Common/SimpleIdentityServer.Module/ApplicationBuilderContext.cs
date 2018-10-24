using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;

namespace SimpleIdentityServer.Module
{
    public class ApplicationBuilderContext
    {
        private IApplicationBuilder _app;
        private IHostingEnvironment _env;
        private ILoggerFactory _loggerFactory;
        private IRouteBuilder _routeBuilder;

        public ApplicationBuilderContext()
        {

        }

        #region Events

        public event EventHandler Initialized;
        public event EventHandler RouteConfigured;

        #endregion

        #region Properties

        public IApplicationBuilder App
        {
            get
            {
                return _app;
            }
        }

        public IHostingEnvironment Env
        {
            get
            {
                return _env;
            }
        }

        public ILoggerFactory LoggerFactory
        {
            get
            {
                return _loggerFactory;
            }
        }

        public IRouteBuilder RouteBuilder
        {
            get
            {
                return _routeBuilder;
            }
        }

        #endregion

        #region Public methods

        public void Init(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            _app = app;
            _env = env;
            _loggerFactory = loggerFactory;
            if (Initialized != null)
            {
                Initialized(this, EventArgs.Empty);
            }
        }

        public void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            _routeBuilder = routeBuilder;
            if (RouteConfigured != null)
            {
                RouteConfigured(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
