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
using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Configuration.IdServer.EF.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.IdServer.EF.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly ConfigurationDbContext _context;
        private readonly IdServerConfigurationDbContext _idServerConfigurationDbContext;
        private readonly IConfigurationEventSource _configurationEventSource;

        public SettingRepository(
            ConfigurationDbContext context,
            IdServerConfigurationDbContext idServerConfigurationDbContext,
            IConfigurationEventSource configurationEventSource)
        {
            _context = context;
            _idServerConfigurationDbContext = idServerConfigurationDbContext;
            _configurationEventSource = configurationEventSource;
        }

        public async Task<ICollection<Core.Models.Setting>> GetAll()
        {
            return await _idServerConfigurationDbContext.Settings.Select(c => c.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Core.Models.Setting> Get(string key)
        {
            var configuration = await _idServerConfigurationDbContext.Settings.FirstOrDefaultAsync(c => c.Key == key).ConfigureAwait(false);
            return configuration == null ? null : configuration.ToDomain();
        }

        public async Task<bool> Insert(Core.Models.Setting configuration)
        {
            if (configuration == null)
            {
                return false;
            }

            using (var transaction = await _idServerConfigurationDbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    _idServerConfigurationDbContext.Settings.Add(new Models.Setting
                    {
                        Key = configuration.Key,
                        Value = configuration.Value
                    });
                    await _idServerConfigurationDbContext.SaveChangesAsync().ConfigureAwait(false);
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

        public async Task<bool> Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            using (var transaction = await _idServerConfigurationDbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var configuration = await _idServerConfigurationDbContext.Settings.FirstOrDefaultAsync(c => c.Key == key).ConfigureAwait(false);
                    if (configuration == null)
                    {
                        return false;
                    }

                    _idServerConfigurationDbContext.Settings.Remove(configuration);
                    await _idServerConfigurationDbContext.SaveChangesAsync().ConfigureAwait(false);
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

        public async Task<bool> Update(Core.Models.Setting conf)
        {
            if (conf == null || string.IsNullOrWhiteSpace(conf.Key))
            {
                return false;
            }

            using (var transaction = await _idServerConfigurationDbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                using (var secondTransaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
                {
                    try
                    {
                        var configuration = await _idServerConfigurationDbContext.Settings.FirstOrDefaultAsync(c => c.Key == conf.Key).ConfigureAwait(false);
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

                        await _idServerConfigurationDbContext.SaveChangesAsync().ConfigureAwait(false);
                        await _context.SaveChangesAsync().ConfigureAwait(false); ;
                        transaction.Commit();
                        secondTransaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _configurationEventSource.Failure(ex);
                        transaction.Rollback();
                        secondTransaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public async Task<ICollection<Core.Models.Setting>> Get(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException(nameof(ids));
            }

            try
            {
                return await _idServerConfigurationDbContext.Settings.Where(s => ids.Contains(s.Key)).Select(r => r.ToDomain()).ToListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _configurationEventSource.Failure(ex);
                return new List<Core.Models.Setting>();
            }
        }

        public async Task<bool> Update(IEnumerable<Core.Models.Setting> settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    foreach (var setting in settings)
                    {
                        var record = await _idServerConfigurationDbContext.Settings.FirstOrDefaultAsync(c => c.Key == setting.Key).ConfigureAwait(false);
                        if (record == null)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        record.Value = setting.Value;
                    }

                    await _context.SaveChangesAsync().ConfigureAwait(false);
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
    }
}
