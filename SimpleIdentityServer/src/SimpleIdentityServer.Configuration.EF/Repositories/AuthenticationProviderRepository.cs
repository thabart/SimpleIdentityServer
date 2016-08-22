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

using Microsoft.EntityFrameworkCore;
using SimpleIdentityServer.Configuration.Core.Models;
using SimpleIdentityServer.Configuration.Core.Repositories;
using SimpleIdentityServer.Configuration.EF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.EF.Repositories
{
    internal class AuthenticationProviderRepository : IAuthenticationProviderRepository
    {
        private readonly SimpleIdentityServerConfigurationContext _simpleIdentityServerConfigurationContext;

        #region Constructor

        public AuthenticationProviderRepository(SimpleIdentityServerConfigurationContext simpleIdentityServerConfigurationContext)
        {
            _simpleIdentityServerConfigurationContext = simpleIdentityServerConfigurationContext;
        }

        #endregion

        #region Public methods

        public async Task<List<AuthenticationProvider>> GetAuthenticationProviders()
        {
            try
            {
                var authProviders = _simpleIdentityServerConfigurationContext.AuthenticationProviders.Include(a => a.Options);
                var result = await authProviders.ToListAsync();
                return result.Select(r => r.ToDomain()).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<AuthenticationProvider> GetAuthenticationProvider(string name)
        {
            try
            {
                var authProviders = _simpleIdentityServerConfigurationContext.AuthenticationProviders.Include(a => a.Options);
                var result = await authProviders.FirstOrDefaultAsync(a => a.Name == name);
                if (result == null)
                {
                    return null;
                }

                return result.ToDomain();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> UpdateAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            try
            {
                var record = await _simpleIdentityServerConfigurationContext
                    .AuthenticationProviders
                    .Include(a => a.Options)
                    .FirstOrDefaultAsync(a => a.Name == authenticationProvider.Name);
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
                    foreach(var opt in authenticationProvider.Options)
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
                foreach(var optId in optionIds.Where(id => !optsNotToBeDeleted.Contains(id)))
                {
                    record.Options.Remove(record.Options.First(o => o.Id == optId));
                }
                
                await _simpleIdentityServerConfigurationContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> AddAuthenticationProvider(AuthenticationProvider authenticationProvider)
        {
            try
            {
                _simpleIdentityServerConfigurationContext.AuthenticationProviders.Add(authenticationProvider.ToModel());
                await _simpleIdentityServerConfigurationContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
