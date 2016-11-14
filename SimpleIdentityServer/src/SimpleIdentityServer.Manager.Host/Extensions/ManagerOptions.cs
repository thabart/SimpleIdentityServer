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

using SimpleIdentityServer.Core.Services;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public class IntrospectOptions
    {
        public string IntrospectionUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

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

    public class ManagerOptions
    {
        /// <summary>
        /// Enable or disable the developer mode
        /// </summary>
        public bool IsDeveloperModeEnabled { get; set; }
        /// <summary>
        /// Configure the introspection options.
        /// </summary>
        public IntrospectOptions Introspection { get; set; }
        /// <summary>
        /// Configure the SERILOG logging.
        /// </summary>
        public LoggingOptions Logging { get; set; }
        /// <summary>
        /// Service used to encrypt the password (for the client).
        /// </summary>
        public IPasswordService PasswordService { get; set; }
        /// <summary>
        /// Service used to authenticate the resource owner.
        /// </summary>
        public IAuthenticateResourceOwnerService AuthenticateResourceOwnerService { get; set; }
    }
}
