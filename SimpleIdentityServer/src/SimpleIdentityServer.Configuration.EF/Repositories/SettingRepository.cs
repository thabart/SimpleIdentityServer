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

using System.Linq;
using SimpleIdentityServer.Configuration.Core.Repositories;
using System.Collections.Generic;
using SimpleIdentityServer.Configuration.EF.Extensions;
using SimpleIdentityServer.Logging;
using System;

namespace SimpleIdentityServer.Configuration.EF.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly SimpleIdentityServerConfigurationContext _context;

        private readonly IConfigurationEventSource _configurationEventSource;

        #region Constructor

        public SettingRepository(
            SimpleIdentityServerConfigurationContext context,
            IConfigurationEventSource configurationEventSource)
        {
            _context = context;
            _configurationEventSource = configurationEventSource;
        }

        #endregion

        #region Public methods

        public List<Core.Models.Setting> GetAll()
        {
            return _context.Settings.Select(c => c.ToDomain()).ToList();
        }

        public Core.Models.Setting Get(string key)
        {
            var configuration = _context.Settings.FirstOrDefault(c => c.Key == key);
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
                _context.Settings.Add(new Models.Setting
                {
                    Key = configuration.Key,
                    Value = configuration.Value
                });
                _context.SaveChanges();
                return true;
            }
            catch(Exception ex)
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
                var configuration = _context.Settings.FirstOrDefault(c => c.Key == key);
                if (configuration == null)
                {
                    return false;
                }

                _context.Settings.Remove(configuration);
                _context.SaveChanges();
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
                var configuration = _context.Settings.FirstOrDefault(c => c.Key == conf.Key);
                configuration.Value = conf.Value;
                _context.SaveChanges();
                return true;
            }
            catch(Exception ex)
            {
                _configurationEventSource.Failure(ex);
                return false;
            }
        }

        #endregion
    }
}
