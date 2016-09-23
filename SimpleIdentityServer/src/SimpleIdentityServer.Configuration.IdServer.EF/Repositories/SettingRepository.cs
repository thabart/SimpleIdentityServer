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

using IdentityServer4.EntityFramework.DbContexts;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Configuration.IdServer.EF.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Configuration.IdServer.EF.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        #region Fields

        private readonly ConfigurationDbContext _context;

        private readonly IdServerConfigurationDbContext _idServerConfigurationDbContext;

        private readonly IConfigurationEventSource _configurationEventSource;

        #endregion

        #region Constructor

        public SettingRepository(
            ConfigurationDbContext context,
            IdServerConfigurationDbContext idServerConfigurationDbContext,
            IConfigurationEventSource configurationEventSource)
        {
            _context = context;
            _idServerConfigurationDbContext = idServerConfigurationDbContext;
            _configurationEventSource = configurationEventSource;
        }

        #endregion

        #region Public methods

        public List<Core.Models.Setting> GetAll()
        {
            return _idServerConfigurationDbContext.Settings.Select(c => c.ToDomain()).ToList();
        }

        public Core.Models.Setting Get(string key)
        {
            var configuration = _idServerConfigurationDbContext.Settings.FirstOrDefault(c => c.Key == key);
            return configuration == null ? null : configuration.ToDomain();
        }

        public bool Insert(Core.Models.Setting configuration)
        {
            if (configuration == null)
            {
                return false;
            }

            try
            {
                _idServerConfigurationDbContext.Settings.Add(new Models.Setting
                {
                    Key = configuration.Key,
                    Value = configuration.Value
                });
                _idServerConfigurationDbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _configurationEventSource.Failure(ex);
                return false;
            }
        }

        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            try
            {
                var configuration = _idServerConfigurationDbContext.Settings.FirstOrDefault(c => c.Key == key);
                if (configuration == null)
                {
                    return false;
                }

                _idServerConfigurationDbContext.Settings.Remove(configuration);
                _idServerConfigurationDbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _configurationEventSource.Failure(ex);
                return false;
            }
        }

        public bool Update(Core.Models.Setting conf)
        {
            if (conf == null || string.IsNullOrWhiteSpace(conf.Key))
            {
                return false;
            }

            try
            {
                var configuration = _idServerConfigurationDbContext.Settings.FirstOrDefault(c => c.Key == conf.Key);
                configuration.Value = conf.Value;
                int lifeTime;
                if ((conf.Key == Core.Constants.SettingNames.ExpirationTimeName ||
                    conf.Key == Core.Constants.SettingNames.AuthorizationCodeExpirationTimeName) && 
                    int.TryParse(conf.Value, out lifeTime))
                {
                    var clients = _context.Clients;
                    foreach (var client in clients)
                    {
                        if (conf.Key == Core.Constants.SettingNames.ExpirationTimeName)
                        {
                            client.AccessTokenLifetime = lifeTime;
                        }
                        if (conf.Key == Core.Constants.SettingNames.AuthorizationCodeExpirationTimeName)
                        {
                            client.AuthorizationCodeLifetime = lifeTime;
                        }
                    }
                }

                _idServerConfigurationDbContext.SaveChanges();
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _configurationEventSource.Failure(ex);
                return false;
            }
        }

        public List<Core.Models.Setting> Get(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            try
            {
                return _idServerConfigurationDbContext.Settings.Where(s => ids.Contains(s.Key)).Select(r => r.ToDomain()).ToList();
            }
            catch (Exception ex)
            {
                _configurationEventSource.Failure(ex);
                return new List<Core.Models.Setting>();
            }
        }

        public bool Update(IEnumerable<Core.Models.Setting> settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var setting in settings)
                    {
                        var record = _idServerConfigurationDbContext.Settings.FirstOrDefault(c => c.Key == setting.Key);
                        if (record == null)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        record.Value = setting.Value;
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    _configurationEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }
        }

        #endregion
    }
}
