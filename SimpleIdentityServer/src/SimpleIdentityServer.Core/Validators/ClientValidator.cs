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

using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Validators
{
    public interface IClientValidator
    {
        Models.Client ValidateClientExist(string clientId);
        Task<Models.Client> ValidateClientExistAsync(string clientId);
        string ValidateRedirectionUrl(string url, Models.Client client);
        bool ValidateGrantType(GrantType grantType, Models.Client client);
        bool ValidateGrantTypes(Models.Client client, params GrantType[] grantTypes);
        bool ValidateResponseType(ResponseType responseType, Models.Client client);
        bool ValidateResponseTypes(IList<ResponseType> responseType, Models.Client client);
    }

    public class ClientValidator : IClientValidator
    {
        private readonly IClientRepository _clientRepository;

        #region Constructor

        public ClientValidator(
            IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        #endregion

        #region Public methods

        public Models.Client ValidateClientExist(string clientId)
        {
            return ValidateClientExistAsync(clientId).Result;
        }

        public async Task<Models.Client> ValidateClientExistAsync(string clientId)
        {
            return await _clientRepository.GetClientByIdAsync(clientId);
        }


        public string ValidateRedirectionUrl(string url, Models.Client client)
        {
            if (string.IsNullOrWhiteSpace(url) ||
                client == null || 
                client.RedirectionUrls == null || 
                !client.RedirectionUrls.Any())
            {
                return null;
            }

            return client.RedirectionUrls.FirstOrDefault(r => r == url);
        }

        public bool ValidateGrantType(GrantType grantType, Models.Client client)
        {
            if (client == null)
            {
                return false;
            }

            SetDefaultClientGrantType(client);
            return client.GrantTypes != null && client.GrantTypes.Contains(grantType);
        }

        public bool ValidateGrantTypes(Models.Client client, params GrantType[] grantTypes)
        {
            if (client == null || grantTypes == null)
            {
                return false;
            }

            SetDefaultClientGrantType(client);
            return client.GrantTypes != null && grantTypes.All(gt => client.GrantTypes.Contains(gt));
        }

        public bool ValidateResponseType(ResponseType responseType, Models.Client client)
        {
            if (client == null)
            {
                return false;
            }

            SetDefaultClientResponseType(client);
            return client.ResponseTypes != null && client.ResponseTypes.Contains(responseType);
        }

        public bool ValidateResponseTypes(IList<ResponseType> responseTypes, Models.Client client)
        {
            if (client == null)
            {
                return false;
            }

            SetDefaultClientResponseType(client);
            return client.ResponseTypes != null && responseTypes.All(rt => client.ResponseTypes.Contains(rt));
        }

        #endregion

        #region Private static methods

        private static void SetDefaultClientGrantType(Models.Client client)
        {
            if (client.GrantTypes == null || !client.GrantTypes.Any())
            {
                client.GrantTypes = new List<GrantType>
                {
                    GrantType.authorization_code
                };
            }
        }

        private static void SetDefaultClientResponseType(Models.Client client)
        {
            if (client.ResponseTypes == null || !client.ResponseTypes.Any())
            {
                client.ResponseTypes = new List<ResponseType>
                {
                    ResponseType.code
                };
            }
        }

        #endregion
    }
}
