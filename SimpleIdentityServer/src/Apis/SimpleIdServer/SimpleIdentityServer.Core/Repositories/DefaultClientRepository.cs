using SimpleIdentityServer.Core.Common.Parameters;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    internal sealed class DefaultClientRepository : IClientRepository
    {
        public ICollection<Common.Models.Client> _clients;

        public DefaultClientRepository(ICollection<Common.Models.Client> clients)
        {
            _clients = clients == null ? new List<Common.Models.Client>() : clients;
        }

        public Task<bool> DeleteAsync(Common.Models.Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            var client = _clients.FirstOrDefault(c => c.ClientId == newClient.ClientId);
            if (client == null)
            {
                return Task.FromResult(false);
            }

            _clients.Remove(client);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<Common.Models.Client>> GetAllAsync()
        {
            return Task.FromResult((IEnumerable<Common.Models.Client>)_clients.Select(c => c.Copy()));
        }

        public Task<Common.Models.Client> GetClientByIdAsync(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var res = _clients.FirstOrDefault(c => c.ClientId == clientId);
            if (res == null)
            {
                return Task.FromResult((Common.Models.Client)null);
            }
            
            return Task.FromResult(res.Copy());
        }

        public Task<bool> InsertAsync(Common.Models.Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            newClient.CreateDateTime = DateTime.UtcNow;
            _clients.Add(newClient.Copy());
            return Task.FromResult(true);
        }

        public Task<bool> RemoveAllAsync()
        {
            _clients.Clear();
            return Task.FromResult(true);
        }

        public Task<SearchClientResult> Search(SearchClientParameter parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }


            IEnumerable<Common.Models.Client> result = _clients;
            if (parameter.ClientIds != null && parameter.ClientIds.Any())
            {
                result = result.Where(c => parameter.ClientIds.Any(i => c.ClientId.Contains(i)));
            }

            if (parameter.ClientNames != null && parameter.ClientNames.Any())
            {
                result = result.Where(c => parameter.ClientNames.Any(n => c.ClientName.Contains(n)));
            }

            if (parameter.ClientTypes != null && parameter.ClientTypes.Any())
            {
                var clientTypes = parameter.ClientTypes.Select(t => (Common.Models.ApplicationTypes)t);
                result = result.Where(c => clientTypes.Contains(c.ApplicationType));
            }

            var nbResult = result.Count();
            if (parameter.Order != null)
            {
                switch (parameter.Order.Target)
                {
                    case "update_datetime":
                        switch (parameter.Order.Type)
                        {
                            case OrderTypes.Asc:
                                result = result.OrderBy(c => c.UpdateDateTime);
                                break;
                            case OrderTypes.Desc:
                                result = result.OrderByDescending(c => c.UpdateDateTime);
                                break;
                        }
                        break;
                }
            }
            else
            {
                result = result.OrderByDescending(c => c.UpdateDateTime);
            }

            if (parameter.IsPagingEnabled)
            {
                result = result.Skip(parameter.StartIndex).Take(parameter.Count);
            }

            return Task.FromResult(new SearchClientResult
            {
                Content = result.Select(s => s.Copy()),
                StartIndex = parameter.StartIndex,
                TotalResults = nbResult
            });
        }

        public Task<bool> UpdateAsync(Common.Models.Client newClient)
        {
            if (newClient == null)
            {
                throw new ArgumentNullException(nameof(newClient));
            }

            var client = _clients.FirstOrDefault(c => c.ClientId == newClient.ClientId);
            client.ClientName = newClient.ClientName;
            client.ClientUri = newClient.ClientUri;
            client.Contacts = newClient.Contacts;
            client.DefaultAcrValues = newClient.DefaultAcrValues;
            client.DefaultMaxAge = newClient.DefaultMaxAge;
            client.GrantTypes = newClient.GrantTypes;
            client.IdTokenEncryptedResponseAlg = newClient.IdTokenEncryptedResponseAlg;
            client.IdTokenEncryptedResponseEnc = newClient.IdTokenEncryptedResponseEnc;
            client.IdTokenSignedResponseAlg = newClient.IdTokenSignedResponseAlg;
            client.InitiateLoginUri = newClient.InitiateLoginUri;
            client.JsonWebKeys = newClient.JsonWebKeys;
            client.JwksUri = newClient.JwksUri;
            client.LogoUri = newClient.LogoUri;
            client.PolicyUri = newClient.PolicyUri;
            client.PostLogoutRedirectUris = newClient.PostLogoutRedirectUris;
            client.RedirectionUrls = newClient.RedirectionUrls;
            client.RequestObjectEncryptionAlg = newClient.RequestObjectEncryptionAlg;
            client.RequestObjectEncryptionEnc = newClient.RequestObjectEncryptionEnc;
            client.RequestObjectSigningAlg = newClient.RequestObjectSigningAlg;
            client.RequestUris = newClient.RequestUris;
            client.RequireAuthTime = newClient.RequireAuthTime;
            client.RequirePkce = newClient.RequirePkce;
            client.ResponseTypes = newClient.ResponseTypes;
            client.ScimProfile = newClient.ScimProfile;
            client.Secrets = newClient.Secrets;
            client.SectorIdentifierUri = newClient.SectorIdentifierUri;
            client.SubjectType = newClient.SubjectType;
            client.TokenEndPointAuthMethod = newClient.TokenEndPointAuthMethod;
            client.TokenEndPointAuthSigningAlg = newClient.TokenEndPointAuthSigningAlg;
            client.TosUri = newClient.TosUri;
            client.UpdateDateTime = DateTime.UtcNow;
            client.UserInfoEncryptedResponseAlg = newClient.UserInfoEncryptedResponseAlg;
            client.UserInfoEncryptedResponseEnc = newClient.UserInfoEncryptedResponseEnc;
            client.UserInfoSignedResponseAlg = newClient.UserInfoSignedResponseAlg;
            return Task.FromResult(true);
        }
    }
}
