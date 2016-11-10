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
using System.Collections.Generic;
using SimpleIdentityServer.Core.Repositories;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    internal class ClaimRepository : IClaimRepository
    {
        private readonly SimpleIdentityServerContext _context;

        public ClaimRepository(SimpleIdentityServerContext context)
        {
            _context = context;
        }
        
        public IList<string> GetAll()
        {
            return _context.Claims.Select(c => c.Code).ToList();
        }

        public bool HasClaim(string name)
        {
            return _context.Claims.Any(c => c.Code == name);
        }
    }
}
