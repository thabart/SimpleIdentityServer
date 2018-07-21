﻿#region copyright
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

using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Configuration.Core.Models;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Configuration.IdServer.EF.Extensions;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.IdServer.EF.Repositories
{
    public class AuthenticationProviderRepository : IAuthenticationProviderRepository
    {
        private readonly IdServerConfigurationDbContext _idServerConfigurationDbContext;
        private readonly IConfigurationEventSource _configurationEventSource;

        public AuthenticationProviderRepository(
            IdServerConfigurationDbContext idServerConfigurationDbContext,
            IConfigurationEventSource configurationEventSource)
        {
            _idServerConfigurationDbContext = idServerConfigurationDbContext;
            _configurationEventSource = configurationEventSource;
        }

        public async Task<ICollection<AuthenticationProvider>> GetAuthenticationProviders()
        {
            try
            {
                return await _idServerConfigurationDbContext.AuthenticationProviders.Include(a => a.Options)
                    .Select(r => r.ToDomain())
                    .ToListAsync()
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _configurationEventSource.Failure(ex);
                return null;
            }
        }

        public async Task<AuthenticationProvider> GetAuthenticationProvider(string name)
        {
            try
            {
                var result = await _idServerConfigurationDbContext.AuthenticationProviders.Include(a => a.Options)
                    .FirstOrDefaultAsync(a => a.Name == name)
                    .ConfigureAwait(false);
                return result == null ? null : result.ToDomain();
            }
            catch (Exception ex)
            {
                _configurationEventSource.Failure(ex);
                return null;
            }
        }

        public async Task<bool> UpdateAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            using (var transaction = await _idServerConfigurationDbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = await _idServerConfigurationDbContext
                        .AuthenticationProviders
                        .Include(a => a.Options)
                        .FirstOrDefaultAsync(a => a.Name == authenticationProvider.Name)
                        .ConfigureAwait(false);
                    if (record == null)
                    {
                        return false;
                    }

                    record.IsEnabled = authenticationProvider.IsEnabled;
                    record.CallbackPath = authenticationProvider.CallbackPath;
                    record.ClassName = authenticationProvider.ClassName;
                    record.Code = authenticationProvider.Code;
                    record.Namespace = authenticationProvider.Namespace;
                    record.Type = authenticationProvider.Type;
                    var optsNotToBeDeleted = new List<string>();
                    if (authenticationProvider.Options != null)
                    {
                        foreach (var opt in authenticationProvider.Options)
                        {
                            var option = record.Options.FirstOrDefault(o => o.Id == opt.Id);
                            if (option != null)
                            {
                                option.Key = opt.Key;
                                option.Value = opt.Value;
                            }
                            else
                            {
                                option = new Models.Option
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    Key = opt.Key,
                                    Value = opt.Value
                                };
                                record.Options.Add(option);
                            }
                            optsNotToBeDeleted.Add(option.Id);
                        }
                    }

                    var optionIds = record.Options.Select(o => o.Id).ToList();
                    foreach (var optId in optionIds.Where(id => !optsNotToBeDeleted.Contains(id)))
                    {
                        record.Options.Remove(record.Options.First(o => o.Id == optId));
                    }

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

        public async Task<bool> AddAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            using (var transaction = await _idServerConfigurationDbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    _idServerConfigurationDbContext.AuthenticationProviders.Add(authenticationProvider.ToModel());
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

        public async Task<bool> RemoveAuthenticationProvider(string name)
        {
            using (var transaction = await _idServerConfigurationDbContext.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var result = await _idServerConfigurationDbContext.AuthenticationProviders.FirstOrDefaultAsync(a => a.Name == name)
                        .ConfigureAwait(false);
                    if (result == null)
                    {
                        return false;
                    }

                    _idServerConfigurationDbContext.AuthenticationProviders.Remove(result);
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
    }
}
