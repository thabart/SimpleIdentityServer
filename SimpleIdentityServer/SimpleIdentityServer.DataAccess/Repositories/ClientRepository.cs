using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.DataAccess.SqlServer.Extensions;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Repositories
{
    public sealed class ClientRepository : IClientRepository
    {
        #region Public methods

        public Core.Models.Client GetClientById(string clientId)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var client = context.Clients
                            .Include(c => c.AllowedScopes)
                            .Include(c => c.JsonWebKeys)
                            .FirstOrDefault(c => c.ClientId == clientId);
                if (client == null)
                {
                    return null;
                }

                return client.ToDomain();
            }
        }

        public bool InsertClient(Core.Models.Client client)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var scopes = new List<Scope>();
                        var jsonWebKeys = new List<JsonWebKey>();
                        var grantTypes = client.GrantTypes == null
                            ? string.Empty
                            : ConcatListOfIntegers(client.GrantTypes.Select(k => (int) k).ToList());
                        var responseTypes = client.ResponseTypes == null
                            ? string.Empty
                            : ConcatListOfIntegers(client.ResponseTypes.Select(r => (int) r).ToList());
                        if (client.AllowedScopes != null)
                        {
                            var scopeNames = scopes.Select(s => s.Name).ToList();
                            scopes = context.Scopes.Where(s => scopeNames.Contains(s.Name))
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

                        var newClient = new Client
                        {
                            ClientId = client.ClientId,
                            ClientName = client.ClientName,
                            ClientUri = client.ClientUri,
                            ClientSecret = client.ClientSecret,
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
                            TokenEndPointAuthMethod = (TokenEndPointAuthenticationMethods)client.TokenEndPointAuthMethod,
                            ApplicationType = (ApplicationTypes)client.ApplicationType,
                            RequestUris = ConcatListOfStrings(client.RequestUris),
                            RedirectionUrls = ConcatListOfStrings(client.RedirectionUrls),
                            Contacts = ConcatListOfStrings(client.Contacts),
                            AllowedScopes = scopes,
                            JsonWebKeys = jsonWebKeys,
                            GrantTypes = grantTypes,
                            ResponseTypes = responseTypes
                        };

                        context.Clients.Add(newClient);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }

            return true;
        }

        public IList<Core.Models.Client> GetAll()
        {
            using (var context = new SimpleIdentityServerContext())
            {
                var clients = context.Clients.ToList();
                return clients.Select(client => client.ToDomain()).ToList();
            }
        }

        public bool DeleteClient(Core.Models.Client client)
        {
            using (var context = new SimpleIdentityServerContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var connectedClient = context.Clients
                            .Include(c => c.AllowedScopes)
                            .Include(c => c.JsonWebKeys)
                            .FirstOrDefault(c => c.ClientId == client.ClientId);
                        if (connectedClient == null)
                        {
                            return false;
                        }

                        context.Clients.Remove(connectedClient);
                        context.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
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

            return string.Concat(list, ',');
        }

        private static string ConcatListOfIntegers(IEnumerable<int> list)
        {
            if (list == null)
            {
                return string.Empty;
            }

            return string.Concat(list, ',');
        }

        #endregion
    }
}
