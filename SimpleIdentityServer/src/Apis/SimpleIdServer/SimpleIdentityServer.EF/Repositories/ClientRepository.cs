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
using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.EF.Extensions;
using SimpleIdentityServer.EF.Models;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.EF.Repositories
{
    public sealed class ClientRepository : IClientRepository
    {
        private readonly SimpleIdentityServerContext _context;
        private readonly ITechnicalEventSource _managerEventSource;

        #region Constructor

        public ClientRepository(
            SimpleIdentityServerContext context,
            ITechnicalEventSource managerEventSource)
        {
            _context = context;
            _managerEventSource = managerEventSource;
        }

        #endregion

        #region Public methods

        public async Task<SearchClientResult> Search(SearchClientParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            IQueryable<Client> clients = _context.Clients;
            if (parameter.ClientIds != null && parameter.ClientIds.Any())
            {
                clients = clients.Where(c => parameter.ClientIds.Any(i => c.ClientId.Contains(i)));
            }

            if (parameter.ClientNames != null && parameter.ClientNames.Any())
            {
                clients = clients.Where(c => parameter.ClientNames.Any(n => c.ClientName.Contains(n)));
            }

            if (parameter.ClientTypes != null && parameter.ClientTypes.Any())
            {
                var clientTypes = parameter.ClientTypes.Select(t => (Models.ApplicationTypes)t);
                clients = clients.Where(c => clientTypes.Contains(c.ApplicationType));
            }

            var nbResult = await clients.CountAsync().ConfigureAwait(false);
            if (parameter.Order != null)
            {
                switch (parameter.Order.Target)
                {
                    case "update_datetime":
                        switch (parameter.Order.Type)
                        {
                            case OrderTypes.Asc:
                                clients = clients.OrderBy(c => c.UpdateDateTime);
                                break;
                            case OrderTypes.Desc:
                                clients = clients.OrderByDescending(c => c.UpdateDateTime);
                                break;
                        }
                        break;
                }
            }
            else
            {
                clients = clients.OrderByDescending(c => c.UpdateDateTime);
            }

            if (parameter.IsPagingEnabled)
            {
                clients = clients.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return new SearchClientResult
            {
                Content = await clients.Select(c => c.ToDomain()).ToListAsync().ConfigureAwait(false),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            };
        }

        public async Task<Core.Common.Models.Client> GetClientByIdAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var client = await _context.Clients
                .Include(c => c.ClientScopes).ThenInclude(s => s.Scope)
                .Include(c => c.JsonWebKeys)
                .Include(c => c.ClientSecrets)
                .FirstOrDefaultAsync(c => c.ClientId == clientId)
                .ConfigureAwait(false);
            if (client == null)
            {
                return null;
            }
            
            return client.ToDomain();
        }

        public async Task<IEnumerable<Core.Common.Models.Client>> GetAllAsync()
        {
            return await _context.Clients
                .Include(c => c.JsonWebKeys)
                .Include(c => c.ClientScopes).ThenInclude(s => s.Scope)
                .Include(c => c.ClientSecrets)
                .Select(c => c.ToDomain())
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<bool> UpdateAsync(Core.Common.Models.Client client)
        {
            using (var translation = _context.Database.BeginTransaction())
            {
                try
                {
                    var grantTypes = client.GrantTypes == null
                        ? string.Empty
                        : ConcatListOfIntegers(client.GrantTypes.Select(k => (int)k).ToList());
                    var responseTypes = client.ResponseTypes == null
                        ? string.Empty
                        : ConcatListOfIntegers(client.ResponseTypes.Select(r => (int)r).ToList());
                    var connectedClient = await _context.Clients
                        .Include(c => c.ClientScopes)
                        .FirstOrDefaultAsync(c => c.ClientId == client.ClientId)
                        .ConfigureAwait(false);
                    connectedClient.ClientName = client.ClientName;
                    connectedClient.ClientUri = client.ClientUri;
                    connectedClient.Contacts = ConcatListOfStrings(client.Contacts);
                    connectedClient.DefaultAcrValues = client.DefaultAcrValues;
                    connectedClient.DefaultMaxAge = client.DefaultMaxAge;
                    connectedClient.GrantTypes = grantTypes;
                    connectedClient.IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg;
                    connectedClient.IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc;
                    connectedClient.IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg;
                    connectedClient.InitiateLoginUri = client.InitiateLoginUri;
                    connectedClient.JwksUri = client.JwksUri;
                    connectedClient.LogoUri = client.LogoUri;
                    connectedClient.PolicyUri = client.PolicyUri;
                    connectedClient.RedirectionUrls = ConcatListOfStrings(client.RedirectionUrls);
                    connectedClient.RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg;
                    connectedClient.RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc;
                    connectedClient.RequestObjectSigningAlg = client.RequestObjectSigningAlg;
                    connectedClient.RequestUris = ConcatListOfStrings(client.RequestUris);
                    connectedClient.PostLogoutRedirectUris = ConcatListOfStrings(client.PostLogoutRedirectUris);
                    connectedClient.RequireAuthTime = client.RequireAuthTime;
                    connectedClient.ResponseTypes = responseTypes;
                    connectedClient.SectorIdentifierUri = client.SectorIdentifierUri;
                    connectedClient.SubjectType = client.SubjectType;
                    connectedClient.TokenEndPointAuthMethod = (TokenEndPointAuthenticationMethods)client.TokenEndPointAuthMethod;
                    connectedClient.TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg;
                    connectedClient.TosUri = client.TosUri;
                    connectedClient.UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg;
                    connectedClient.UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc;
                    connectedClient.UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg;
                    connectedClient.ScimProfile = client.ScimProfile;
                    connectedClient.UpdateDateTime = DateTime.UtcNow;
                    var scopesNotToBeDeleted = new List<string>();
                    if (client.AllowedScopes != null)
                    {
                        foreach(var scope in client.AllowedScopes)
                        {
                            var record = connectedClient.ClientScopes.FirstOrDefault(c => c.ScopeName == scope.Name);
                            if (record == null)
                            {
                                record = new ClientScope
                                {
                                    ClientId = connectedClient.ClientId,
                                    ScopeName = scope.Name
                                };
                                connectedClient.ClientScopes.Add(record);
                            }

                            scopesNotToBeDeleted.Add(record.ScopeName);
                        }
                    }

                    var scopeNames = connectedClient.ClientScopes.Select(o => o.ScopeName).ToList();
                    foreach (var scopeName in scopeNames.Where(id => !scopesNotToBeDeleted.Contains(id)))
                    {
                        connectedClient.ClientScopes.Remove(connectedClient.ClientScopes.First(s => s.ScopeName == scopeName));
                    }

                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    translation.Commit();
                }
                catch(Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    translation.Rollback();
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> RemoveAllAsync()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.Clients.RemoveRange(_context.Clients);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public async Task<bool> InsertAsync(Core.Common.Models.Client client)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var scopes = new List<ClientScope>();
                    var jsonWebKeys = new List<JsonWebKey>();
                    var clientSecrets = new List<ClientSecret>();
                    var grantTypes = client.GrantTypes == null
                        ? string.Empty
                        : ConcatListOfIntegers(client.GrantTypes.Select(k => (int)k).ToList());
                    var responseTypes = client.ResponseTypes == null
                        ? string.Empty
                        : ConcatListOfIntegers(client.ResponseTypes.Select(r => (int)r).ToList());
                    if (client.AllowedScopes != null)
                    {
                        var scopeNames = client.AllowedScopes.Select(s => s.Name).ToList();
                        scopes = _context.Scopes.Where(s => scopeNames.Contains(s.Name))
                            .Select(s => new ClientScope { ScopeName = s.Name })
                            .ToList();
                    }

                    if (client.JsonWebKeys != null)
                    {
                        client.JsonWebKeys.ForEach(jsonWebKey =>
                        {
                            var jsonWebKeyRecord = new JsonWebKey
                            {
                                Kid = jsonWebKey.Kid,
                                Use = (Use)jsonWebKey.Use,
                                Kty = (KeyType)jsonWebKey.Kty,
                                SerializedKey = jsonWebKey.SerializedKey,
                                X5t = jsonWebKey.X5t,
                                X5tS256 = jsonWebKey.X5tS256,
                                X5u = jsonWebKey.X5u == null ? string.Empty : jsonWebKey.X5u.AbsoluteUri,
                                Alg = (AllAlg)jsonWebKey.Alg,
                                KeyOps = jsonWebKey.KeyOps == null ? string.Empty : ConcatListOfIntegers(jsonWebKey.KeyOps.Select(k => (int)k).ToList())
                            };

                            jsonWebKeys.Add(jsonWebKeyRecord);
                        });
                    }

                    if (client.Secrets != null)
                    {
                        foreach(var secret in client.Secrets)
                        {
                            clientSecrets.Add(new ClientSecret
                            {
                                Id = Guid.NewGuid().ToString(),
                                ClientId = client.ClientId,
                                Type = (SecretTypes)secret.Type,
                                Value = secret.Value
                            });
                        }
                    }

                    var newClient = new Models.Client
                    {
                        ClientId = client.ClientId,
                        ClientName = client.ClientName,
                        ClientUri = client.ClientUri,
                        IdTokenEncryptedResponseAlg = client.IdTokenEncryptedResponseAlg,
                        IdTokenEncryptedResponseEnc = client.IdTokenEncryptedResponseEnc,
                        JwksUri = client.JwksUri,
                        TosUri = client.TosUri,
                        LogoUri = client.LogoUri,
                        PolicyUri = client.PolicyUri,
                        RequestObjectEncryptionAlg = client.RequestObjectEncryptionAlg,
                        RequestObjectEncryptionEnc = client.RequestObjectEncryptionEnc,
                        IdTokenSignedResponseAlg = client.IdTokenSignedResponseAlg,
                        RequireAuthTime = client.RequireAuthTime,
                        SectorIdentifierUri = client.SectorIdentifierUri,
                        SubjectType = client.SubjectType,
                        TokenEndPointAuthSigningAlg = client.TokenEndPointAuthSigningAlg,
                        UserInfoEncryptedResponseAlg = client.UserInfoEncryptedResponseAlg,
                        UserInfoSignedResponseAlg = client.UserInfoSignedResponseAlg,
                        UserInfoEncryptedResponseEnc = client.UserInfoEncryptedResponseEnc,
                        DefaultMaxAge = client.DefaultMaxAge,
                        DefaultAcrValues = client.DefaultAcrValues,
                        InitiateLoginUri = client.InitiateLoginUri,
                        RequestObjectSigningAlg = client.RequestObjectSigningAlg,
                        TokenEndPointAuthMethod = (Models.TokenEndPointAuthenticationMethods)client.TokenEndPointAuthMethod,
                        ApplicationType = (Models.ApplicationTypes)client.ApplicationType,
                        RequestUris = ConcatListOfStrings(client.RequestUris),
                        RedirectionUrls = ConcatListOfStrings(client.RedirectionUrls),
                        Contacts = ConcatListOfStrings(client.Contacts),
                        PostLogoutRedirectUris = ConcatListOfStrings(client.PostLogoutRedirectUris),
                        ClientScopes = scopes,
                        JsonWebKeys = jsonWebKeys,
                        GrantTypes = grantTypes,
                        ResponseTypes = responseTypes,
                        ScimProfile = client.ScimProfile,
                        ClientSecrets = clientSecrets,
                        CreateDateTime = DateTime.UtcNow,
                        UpdateDateTime = DateTime.UtcNow
                    };

                    _context.Clients.Add(newClient);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }

        public async Task<bool> DeleteAsync(Core.Common.Models.Client client)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var connectedClient = await _context.Clients
                        .Include(c => c.ClientScopes)
                        .Include(c => c.JsonWebKeys)
                        .Include(c => c.ClientSecrets)
                        .FirstOrDefaultAsync(c => c.ClientId == client.ClientId)
                        .ConfigureAwait(false);
                    if (connectedClient == null)
                    {
                        return false;
                    }

                    _context.Clients.Remove(connectedClient);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _managerEventSource.Failure(ex);
                    transaction.Rollback();
                    return false;
                }
            }

            return true;
        }
        
        #endregion

        #region Private static methods

        private static string ConcatListOfStrings(IEnumerable<string> list)
        {
            if (list == null)
            {
                return string.Empty;
            }

            return string.Join(",", list);
        }

        private static string ConcatListOfIntegers(IEnumerable<int> list)
        {
            if (list == null)
            {
                return string.Empty;
            }

            return string.Join(",", list);
        }

        #endregion
    }
}
