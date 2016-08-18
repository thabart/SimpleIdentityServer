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

using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Models;
using System;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly SimpleIdentityServerContext _context;

        #region Constructor

        public ConfigurationRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }

        #endregion

        #region Public methods

        public List<Core.Models.Configuration> GetAll()
        {
            return _context.Configurations.Select(c => c.ToDomain()).ToList();
        }

        public Core.Models.Configuration Get(string key)
        {
            var configuration = _context.Configurations.FirstOrDefault(c => c.Key == key);
            return configuration == null ? null : configuration.ToDomain();
        }

        public bool Insert(Core.Models.Configuration configuration)
        {
            if (configuration == null)
            {
                return false;
            }

            try
            {
                _context.Configurations.Add(new Models.Configuration
                {
                    Key = configuration.Key,
                    Value  = configuration.Value
                });
                _context.SaveChanges();
                return true;
            }
            catch
            {
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
                var configuration = _context.Configurations.FirstOrDefault(c => c.Key == key);
                if (configuration == null)
                {
                    return false;
                }

                _context.Configurations.Remove(configuration);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Update(Configuration conf)
        {
            if (conf == null || string.IsNullOrWhiteSpace(conf.Key))
            {
                return false;
            }

            try
            {
                var configuration = _context.Configurations.FirstOrDefault(c => c.Key == conf.Key);
                configuration.Value = conf.Value;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
