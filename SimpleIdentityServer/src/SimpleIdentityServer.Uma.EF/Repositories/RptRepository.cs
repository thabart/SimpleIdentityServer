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
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.EF.Extensions;

namespace SimpleIdentityServer.Uma.EF.Repositories
{
    internal class RptRepository : IRptRepository
    {
        private readonly SimpleIdServerUmaContext _simpleIdServerUmaContext;

        #region Constructor

        public RptRepository(SimpleIdServerUmaContext simpleIdServerUmaContext)
        {
            _simpleIdServerUmaContext = simpleIdServerUmaContext;
        }

        #endregion

        #region Public methods

        public bool InsertRpt(Rpt rpt)
        {
            var record = rpt.ToModel();
            _simpleIdServerUmaContext.Add(record);
            _simpleIdServerUmaContext.SaveChanges();
            return true;
        }

        public Rpt GetRpt(string value)
        {
            var record = _simpleIdServerUmaContext.Rpts.FirstOrDefault(r => r.Value == value);
            if (record == null)
            {
                return null;
            }

            return record.ToDomain();
        }

        #endregion
    }
}