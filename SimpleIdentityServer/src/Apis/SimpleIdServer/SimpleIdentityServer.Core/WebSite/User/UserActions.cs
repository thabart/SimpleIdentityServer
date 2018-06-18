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

using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User
{
    public interface IUserActions
    {
        Task<IEnumerable<Common.Models.Consent>> GetConsents(ClaimsPrincipal claimsPrincipal);
        Task<bool> DeleteConsent(string consentId);
        Task<ResourceOwner> GetUser(ClaimsPrincipal claimsPrincipal);
        Task<bool> UpdateCredentials(string subject, string newPassword);
        Task<bool> UpdateClaims(string subject, IEnumerable<ClaimAggregate> claims);
        Task<bool> AddUser(AddUserParameter addUserParameter, AuthenticationParameter authenticationParameter, string scimBaseUrl = null, bool addScimResource = false, string issuer = null);
    }

    internal class UserActions : IUserActions
    {
        private readonly IGetConsentsOperation _getConsentsOperation;
        private readonly IRemoveConsentOperation _removeConsentOperation;
        private readonly IGetUserOperation _getUserOperation;
        private readonly IUpdateUserCredentialsOperation _updateUserCredentialsOperation;
        private readonly IUpdateUserClaimsOperation _updateUserClaimsOperation;
        private readonly IAddUserOperation _addUserOperation;

        public UserActions(
            IGetConsentsOperation getConsentsOperation,
            IRemoveConsentOperation removeConsentOperation,
            IGetUserOperation getUserOperation,
            IUpdateUserCredentialsOperation updateUserCredentialsOperation,
            IUpdateUserClaimsOperation updateUserClaimsOperation,
            IAddUserOperation addUserOperation)
        {
            _getConsentsOperation = getConsentsOperation;
            _removeConsentOperation = removeConsentOperation;
            _getUserOperation = getUserOperation;
            _updateUserCredentialsOperation = updateUserCredentialsOperation;
            _updateUserClaimsOperation = updateUserClaimsOperation;
            _addUserOperation = addUserOperation;
        }

        public Task<IEnumerable<Common.Models.Consent>> GetConsents(ClaimsPrincipal claimsPrincipal)
        {
            return _getConsentsOperation.Execute(claimsPrincipal);
        }

        public Task<bool> DeleteConsent(string consentId)
        {
            return _removeConsentOperation.Execute(consentId);
        }

        public Task<ResourceOwner> GetUser(ClaimsPrincipal claimsPrincipal)
        {
            return _getUserOperation.Execute(claimsPrincipal);
        }

        public Task<bool> UpdateCredentials(string subject, string newPassword)
        {
            return _updateUserCredentialsOperation.Execute(subject, newPassword);
        }

        public Task<bool> UpdateClaims(string subject, IEnumerable<ClaimAggregate> claims)
        {
            return _updateUserClaimsOperation.Execute(subject, claims);
        }

        public Task<bool> AddUser(AddUserParameter addUserParameter, AuthenticationParameter authenticationParameter, string scimBaseUrl = null, bool addScimResource = false, string issuer = null)
        {
            return _addUserOperation.Execute(addUserParameter, authenticationParameter, scimBaseUrl, addScimResource, issuer);
        }
    }
}
