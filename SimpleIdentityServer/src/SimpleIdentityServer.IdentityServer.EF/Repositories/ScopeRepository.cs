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
using System.Linq;
using System.Collections.Generic;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using IdentityServer4.EntityFramework.Interfaces;
using SimpleIdentityServer.Logging;
using IdentityServer4.EntityFramework.DbContexts;

namespace SimpleIdentityServer.IdentityServer.EF
{
    internal sealed class ScopeRepository : IScopeRepository
    {
        #region Fields

        private readonly IConfigurationDbContext _context;

        private readonly IManagerEventSource _managerEventSource;

        #endregion

        #region Constructor

        public ScopeRepository(
            ConfigurationDbContext context,
            IManagerEventSource managerEventSource)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
            _managerEventSource = managerEventSource;
        }

        #endregion

        #region Public methods

        public bool DeleteScope(Scope scope)
        {
            throw new NotImplementedException();
        }

        public IList<Scope> GetAllScopes()
        {
            return _context.Scopes.Select(s => s.ToDomain()).ToList();
        }

        public Scope GetScopeByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool InsertScope(Scope scope)
        {
            throw new NotImplementedException();
        }

        public bool UpdateScope(Scope scope)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
