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
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Uma.EF.Repositories
{
    internal class PolicyRepository : IPolicyRepository
    {
        private readonly SimpleIdServerUmaContext _context;

        public PolicyRepository(SimpleIdServerUmaContext context)
        {
            _context = context;
        }

        public async Task<ICollection<Policy>> GetAll()
        {
            return await _context.Policies.Select(p => p.ToDomain()).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Policy> Get(string id)
        {
            var policy = await _context.Policies
                .Include(p => p.PolicyResources)
                .Include(p => p.Rules)
                .FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
            return policy == null ? null : policy.ToDomain();
        }

#if NET46
        public async Task<bool> BulkAdd(IEnumerable<Policy> parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync().ConfigureAwait(false);
            }

            var policyDataTable = new DataTable();
            policyDataTable.Columns.Add("Id", typeof(string));
            var policyResourceDataTable = new DataTable();
            policyResourceDataTable.Columns.Add("PolicyId", typeof(string));
            policyResourceDataTable.Columns.Add("ResourceSetId", typeof(string));
            var policyRuleDataTable = new DataTable();
            policyRuleDataTable.Columns.Add("Id", typeof(string));
            policyRuleDataTable.Columns.Add("Claims", typeof(string));
            policyRuleDataTable.Columns.Add("ClientIdsAllowed", typeof(string));
            policyRuleDataTable.Columns.Add("IsResourceOwnerConsentNeeded", typeof(bool));
            policyRuleDataTable.Columns.Add("PolicyId", typeof(string));
            policyRuleDataTable.Columns.Add("Scopes", typeof(string));
            policyRuleDataTable.Columns.Add("Script", typeof(string));
            foreach(var record in parameter)
            {
                var model = record.ToModel();
                var policyRow = policyDataTable.NewRow();
                policyRow["Id"] = model.Id;
                policyDataTable.Rows.Add(policyRow);
                if (model.PolicyResources != null)
                {
                    foreach(var policyResource in model.PolicyResources)
                    {
                        var policyResourceRow = policyResourceDataTable.NewRow();
                        policyResourceRow["PolicyId"] = record.Id;
                        policyResourceRow["ResourceSetId"] = policyResource.ResourceSetId;
                        policyResourceDataTable.Rows.Add(policyResourceRow);
                    }
                }

                if (model.Rules != null)
                {
                    foreach(var rule in model.Rules)
                    {
                        var policyRuleRow = policyRuleDataTable.NewRow();
                        policyRuleRow["Id"] = rule.Id;
                        policyRuleRow["Claims"] = rule.Claims;
                        policyRuleRow["ClientIdsAllowed"] = rule.ClientIdsAllowed;
                        policyRuleRow["IsResourceOwnerConsentNeeded"] = rule.IsResourceOwnerConsentNeeded;
                        policyRuleRow["PolicyId"] = model.Id;
                        policyRuleRow["Scopes"] = rule.Scopes;
                        policyRuleRow["Script"] = rule.Script;
                        policyRuleDataTable.Rows.Add(policyRuleRow);
                    }
                }
            }

            using (var bulkCopy = new SqlBulkCopy((connection) as SqlConnection, SqlBulkCopyOptions.Default, null))
            {
                bulkCopy.DestinationTableName = "[dbo].[Policies]";
                await bulkCopy.WriteToServerAsync(policyDataTable).ConfigureAwait(false);
            }

            using (var bulkCopy = new SqlBulkCopy((connection) as SqlConnection, SqlBulkCopyOptions.Default, null))
            {
                bulkCopy.DestinationTableName = "[dbo].[PolicyResource]";
                await bulkCopy.WriteToServerAsync(policyResourceDataTable).ConfigureAwait(false);
            }

            using (var bulkCopy = new SqlBulkCopy((connection) as SqlConnection, SqlBulkCopyOptions.Default, null))
            {
                bulkCopy.DestinationTableName = "[dbo].[PolicyRules]";
                await bulkCopy.WriteToServerAsync(policyRuleDataTable).ConfigureAwait(false);
            }

            connection.Close();
            return true;
        }
#endif

        public async Task<bool> Add(Policy policy)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Policies.Add(policy.ToModel());
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> Delete(string id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var policy = await _context.Policies
                        .Include(p => p.Rules)
                        .FirstOrDefaultAsync(p => p.Id == id).ConfigureAwait(false);
                    if (policy == null)
                    {
                        return false;
                    }

                    _context.Policies.Remove(policy);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                } catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> Update(Policy policy)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false))
            {
                try
                {
                    var record = await _context.Policies
                        .FirstOrDefaultAsync(p => p.Id == policy.Id).ConfigureAwait(false);
                    if (record == null)
                    {
                        return false;
                    }

                    var rulesNotToBeDeleted = new List<string>();
                    if (policy.Rules != null)
                    {
                        foreach (var ru in policy.Rules)
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
                            rule.Claims = JsonConvert.SerializeObject(ru.Claims == null ? new List<Claim>() : ru.Claims);
                            rulesNotToBeDeleted.Add(rule.Id);
                        }
                    }

                    var ruleIds = record.Rules.Select(o => o.Id).ToList();
                    foreach (var ruleId in ruleIds.Where(id => !rulesNotToBeDeleted.Contains(id)))
                    {
                        var removedRule = record.Rules.First(o => o.Id == ruleId);
                        record.Rules.Remove(removedRule);
                        _context.PolicyRules.Remove(removedRule);
                    }

                    var resourceSetIdsNotToBeDeleted = new List<string>();
                    if (policy.ResourceSetIds != null)
                    {
                        foreach (var resourceSetId in policy.ResourceSetIds)
                        {
                            var policyResource = record.PolicyResources.FirstOrDefault(p => p.ResourceSetId == resourceSetId);
                            if (policyResource == null)
                            {
                                policyResource = new Models.PolicyResource
                                {
                                    ResourceSetId = resourceSetId,
                                    PolicyId = policy.Id
                                };
                                record.PolicyResources.Add(policyResource);
                            }

                            resourceSetIdsNotToBeDeleted.Add(policyResource.ResourceSetId);
                        }
                    }

                    var resourceSetIds = record.PolicyResources.Select(o => o.ResourceSetId).ToList();
                    foreach (var resourceSetId in resourceSetIds.Where(id => !resourceSetIdsNotToBeDeleted.Contains(id)))
                    {
                        var removedResource = record.PolicyResources.First(o => o.ResourceSetId == resourceSetId);
                        record.PolicyResources.Remove(removedResource);
                    }

                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<ICollection<Policy>> SearchByResourceId(string resourceSetId)
        {
            return await _context.Policies
                .Include(p => p.PolicyResources)
                .Where(p => p.PolicyResources.Any(r => r.ResourceSetId == resourceSetId))
                .Select(p => p.ToDomain())
                .ToListAsync();
        }
    }
}