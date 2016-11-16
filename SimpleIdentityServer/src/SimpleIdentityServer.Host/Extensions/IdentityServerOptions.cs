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
using SimpleIdentityServer.Core.Services;
using System;

namespace SimpleIdentityServer.Host
{
    public sealed class LoggingOptions
    {
        public FileLogOptions FileLogOptions { get; set; }

        public ElasticsearchOptions ElasticsearchOptions { get; set; }
    }

    public sealed class FileLogOptions
    {
        #region Constructor

        public FileLogOptions()
        {
            PathFormat = "log-{Date}.txt";
        }

        #endregion

        #region Properties

        public bool IsEnabled { get; set; }

        public string PathFormat { get; set; }

        #endregion
    }

    public sealed class ElasticsearchOptions
    {
        #region Constructor

        public ElasticsearchOptions()
        {
            Url = "http://localhost:9200";
        }

        #endregion

        #region Properties

        public bool IsEnabled { get; set; }

        public string Url { get; set; }

        #endregion
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
        public bool IsDataMigrated { get; set; }
        /// <summary>
        /// Choose the type of your DataSource
        /// </summary>
        public DataSourceTypes DataSourceType { get; set; }

        /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString { get; set; }
    }

    public class AuthenticateOptions
    {
        public string CookieName = CookieAuthenticationDefaults.AuthenticationScheme;
    }

    public class IdentityServerOptions
    {
        public IdentityServerOptions()
        {
            Authenticate = new AuthenticateOptions();
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
        /// Service used to authenticate the resource owner.
        /// </summary>
        public Type AuthenticateResourceOwner { get; set; }
        /// <summary>
        /// Service used for Two factor authentication (send a validation token).
        /// </summary>
        public Type TwoFactorServiceStore { get; set; }
        /// <summary>
        /// Service used to retrieve configurations (expiration date time etc ...)
        /// </summary>
        public Type ConfigurationService { get; set; }
        /// <summary>
        /// Service used to encrypt the password
        /// </summary>
        public Type PasswordService { get; set; }
    }
}
