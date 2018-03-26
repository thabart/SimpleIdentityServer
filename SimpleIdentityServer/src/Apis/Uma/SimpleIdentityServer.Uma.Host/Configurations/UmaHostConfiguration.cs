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

using System;

namespace SimpleIdentityServer.Uma.Host.Configurations
{
    public enum CachingTypes
    {
        INMEMORY,
        REDIS
    }
    
    public enum DbTypes
    {
        SQLSERVER,
        POSTGRES,
        INMEMORY
    }

    public sealed class DataSourceOptions
    {
        public DataSourceOptions()
        {
            IsUmaMigrated = true;
            IsOauthMigrated = true;
            UmaDbType = DbTypes.INMEMORY;
            OauthDbType = DbTypes.INMEMORY;
        }

        public bool IsUmaMigrated { get; set; }
        public bool IsOauthMigrated { get; set; }
        public DbTypes UmaDbType { get; set; }
        public DbTypes OauthDbType { get; set; }
        public string UmaConnectionString { get; set; }
        public string OauthConnectionString { get; set; }
        public string EvtStoreConnectionString { get; set; }
    }

    public sealed class FileLogsOptions
    {
        public FileLogsOptions()
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

    public sealed class OauthOptions
    {
        public string IntrospectionEndpoints { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }        

    public sealed class CachingOptions
    {
        public CachingOptions()
        {
            Type = CachingTypes.INMEMORY;
        }

        public CachingTypes Type { get; set; }
        public string ConnectionString { get; set; }
        public string InstanceName { get; set; }
        public int Port { get; set; }
    }

    public class UmaHostConfiguration
    {
        public UmaHostConfiguration()
        {
            AuthorizationServer = new OauthOptions();
            DataSource = new DataSourceOptions();
            ResourceCaching = new CachingOptions();
            FileLog = new FileLogsOptions();
            Elasticsearch = new ElasticsearchOptions();
            Storage = new CachingOptions();
        }

        /// <summary>
        /// Get or set the authorization server options.
        /// </summary>
        public OauthOptions AuthorizationServer { get; set; }
        /// <summary>
        /// Get or set the datasource.
        /// </summary>
        public DataSourceOptions DataSource { get; set; }
        /// <summary>
        /// Get or set the caching options.
        /// </summary>
        public CachingOptions ResourceCaching { get; set; }
        /// <summary>
        /// Get or set the storage options (access token + authorization code ...).
        /// </summary>
        public CachingOptions Storage { get; set; }
        /// <summary>
        /// Get or set the file log options.
        /// </summary>
        public FileLogsOptions FileLog { get; set; }
        /// <summary>
        /// Get or set the elastic search options.
        /// </summary>
        public ElasticsearchOptions Elasticsearch { get; set; }
        /// <summary>
        /// Get or set the OPENID well known configuration.
        /// </summary>
        public string OpenIdWellKnownConfiguration { get; set; }
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
    }
}
