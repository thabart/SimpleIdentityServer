#region copyright
// Copyright 2015 Habart Thierry
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.AspNetCore.Authentication.Cookies;
using SimpleIdentityServer.Core.Bus;
using SimpleIdentityServer.Core.Services;
using System;
using System.Collections.Generic;

namespace SimpleIdentityServer.Host
{
    public sealed class LoggingOptions
    {
        public LoggingOptions()
        {
            FileLogOptions = new FileLogOptions();
            ElasticsearchOptions = new ElasticsearchOptions();
        }

        public FileLogOptions FileLogOptions { get; set; }
        public ElasticsearchOptions ElasticsearchOptions { get; set; }
    }

    public sealed class FileLogOptions
    {
        public FileLogOptions()
        {
            IsEnabled = false;
            PathFormat = "log-{Date}.txt";
        }

        public bool IsEnabled { get; set; }
        public string PathFormat { get; set; }
    }

    public sealed class ElasticsearchOptions
    {
        public ElasticsearchOptions()
        {
            IsEnabled = false;
            Url = "http://localhost:9200";
        }

        public bool IsEnabled { get; set; }
        public string Url { get; set; }
    }

    public enum DataSourceTypes
    {
        SqlServer,
        SqlLite,
        Postgre,
        InMemory
    }

    public sealed class DataSourceOptions
    {
        public DataSourceOptions()
        {
            IsOpenIdDataMigrated = true;
            IsEvtStoreDataMigrated = true;
            OpenIdDataSourceType = DataSourceTypes.InMemory;
            EvtStoreDataSourceType = DataSourceTypes.InMemory;
        }

        public bool IsOpenIdDataMigrated { get; set; }
        public bool IsEvtStoreDataMigrated { get; set; }
        /// <summary>
        /// Choose the type of your DataSource
        /// </summary>
        public DataSourceTypes OpenIdDataSourceType { get; set; }
        /// <summary>
        /// Connection string
        /// </summary>
        public string OpenIdConnectionString { get; set; }
        public DataSourceTypes EvtStoreDataSourceType { get; set; }
        public string EvtStoreConnectionString { get; set; }
    }

    public class AuthenticateOptions
    {
        public string CookieName = CookieAuthenticationDefaults.AuthenticationScheme;
    }

    public class ScimOptions
    {
        public string EndPoint { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class EventOptions
    {
        public Type Publisher { get; set; }
        public IEnumerable<IHandler> Handlers { get; set; }
    }

    public class IdentityServerOptions
    {
        public IdentityServerOptions()
        {
            Authenticate = new AuthenticateOptions();
            Scim = new ScimOptions();
            Logging = new LoggingOptions();
            DataSource = new DataSourceOptions();
        }
        /// <summary>
        /// Enable or disable the developer mode
        /// </summary>
        public bool IsDeveloperModeEnabled { get; set; }
        /// <summary>
        /// Configure the data source.
        /// </summary>
        public DataSourceOptions DataSource { get; set; }
        /// <summary>
        /// Configure the SERILOG logging.
        /// </summary>
        public LoggingOptions Logging { get; set; }
        /// <summary>
        /// Configure authentication.
        /// </summary>
        public AuthenticateOptions Authenticate { get; set; }
        /// <summary>
        /// Scim options.
        /// </summary>
        public ScimOptions Scim { get; set; }
        /// <summary>
        /// Configure the event publisher &|or handlers.
        /// </summary>
        public EventOptions Event { get; set; }
        /// <summary>
        /// Service used to authenticate the resource owner.
        /// </summary>
        public Type AuthenticateResourceOwner { get; set; }
        /// <summary>
        /// Service used to retrieve configurations (expiration date time etc ...)
        /// </summary>
        public Type ConfigurationService { get; set; }
        /// <summary>
        /// Service used to encrypt the password
        /// </summary>
        public Type PasswordService { get; set; }
        /// <summary>
        /// Store the two factor authentication methods.
        /// </summary>
        public ITwoFactorServiceStore TwoFactorServiceStore { get; set; }
    }
}
