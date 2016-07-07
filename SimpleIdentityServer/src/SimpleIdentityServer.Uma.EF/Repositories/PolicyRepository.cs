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
using Newtonsoft.Json;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.EF.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleIdentityServer.Uma.EF.Repositories
{
    internal class PolicyRepository : IPolicyRepository
    {
        private readonly SimpleIdServerUmaContext _simpleIdServerUmaContext;

        #region Constructor

        public PolicyRepository(SimpleIdServerUmaContext simpleIdServerUmaContext)
        {
            _simpleIdServerUmaContext = simpleIdServerUmaContext;
        }

        #endregion

        #region Public methods

        public List<Policy> GetPolicies()
        {
            var policies = _simpleIdServerUmaContext.Policies.ToList();
            if (policies == null ||
                !policies.Any())
            {
                return new List<Policy>();
            }

            return policies.Select(p => p.ToDomain()).ToList();
        }

        public Policy GetPolicy(string id)
        {
            var policy = _simpleIdServerUmaContext.Policies
                .Include(p => p.ResourceSets)
                .Include(p => p.Rules)
                .FirstOrDefault(p => p.Id == id);
            if (policy == null)
            {
                return null;
            }

            return policy.ToDomain();
        }

        public bool AddPolicy(Policy policy)
        {
            var record = policy.ToModel();
            _simpleIdServerUmaContext.Policies.Add(record);
            _simpleIdServerUmaContext.SaveChanges();
            return true;
        }

        public bool DeletePolicy(string id)
        {
            var policy = _simpleIdServerUmaContext.Policies
                .Include(p => p.Rules)
                .FirstOrDefault(p => p.Id == id);
            if (policy == null)
            {
                return false;
            }

            _simpleIdServerUmaContext.Policies.Remove(policy);
            _simpleIdServerUmaContext.SaveChanges();
            return true;
        }

        public bool UpdatePolicy(Policy policy)
        {
            var record = _simpleIdServerUmaContext.Policies
                .FirstOrDefault(p => p.Id == policy.Id);
            if (record == null)
            {
                return false;
            }

            var model = policy.ToModel();
            record.ClientIdsAllowed = model.ClientIdsAllowed;
            record.IsResourceOwnerConsentNeeded = model.IsResourceOwnerConsentNeeded;
            record.Scopes = model.Scopes;
            record.Script = model.Script;
            record.Claims = model.Claims;
            var rulesNotToBeDeleted = new List<string>();
            if (policy.Rules != null)
            {
                foreach(var ru in policy.Rules)
                {
                    var rule = record.Rules.FirstOrDefault(r => r.Id == ru.Id);
                    if (rule == null)
                    {
                        rule = new Models.PolicyRule
                        {
                            Id = Guid.NewGuid().ToString(),
                            PolicyId = policy.Id
                        };
                        record.Rules.Add(rule);                   
                    }

                    rule.IsResourceOwnerConsentNeeded = ru.IsResourceOwnerConsentNeeded;
                    rule.Script = ru.Script;
                    rule.ClientIdsAllowed = MappingExtensions.GetConcatenatedList(ru.ClientIdsAllowed);
                    rule.Scopes = MappingExtensions.GetConcatenatedList(ru.Scopes);
                    if (ru.Claims != null &&
                        ru.Claims.Any())
                    {
                        rule.Claims = JsonConvert.SerializeObject(ru.Claims);
                    }

                    rulesNotToBeDeleted.Add(rule.Id);
                }
            }

            var ruleIds = record.Rules.Select(o => o.Id).ToList();
            foreach (var ruleId in ruleIds.Where(id => !rulesNotToBeDeleted.Contains(id)))
            {
                record.Rules.Remove(record.Rules.First(o => o.Id == ruleId));
            }

            _simpleIdServerUmaContext.SaveChanges();
            return true;
        }

        #endregion
    }
}