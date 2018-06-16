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
        Task<bool> UpdateUser(UpdateUserParameter updateUserParameter);
        Task<bool> AddUser(AddUserParameter addUserParameter);
    }

    internal class UserActions : IUserActions
    {
        private readonly IGetConsentsOperation _getConsentsOperation;
        private readonly IRemoveConsentOperation _removeConsentOperation;
        private readonly IGetUserOperation _getUserOperation;
        private readonly IUpdateUserOperation _updateUserOperation;
        private readonly IAddUserOperation _addUserOperation;

        public UserActions(
            IGetConsentsOperation getConsentsOperation,
            IRemoveConsentOperation removeConsentOperation,
            IGetUserOperation getUserOperation,
            IUpdateUserOperation updateUserOperation,
            IAddUserOperation addUserOperation)
        {
            _getConsentsOperation = getConsentsOperation;
            _removeConsentOperation = removeConsentOperation;
            _getUserOperation = getUserOperation;
            _updateUserOperation = updateUserOperation;
            _addUserOperation = addUserOperation;
        }

        public async Task<IEnumerable<Common.Models.Consent>> GetConsents(ClaimsPrincipal claimsPrincipal)
        {
            return await _getConsentsOperation.Execute(claimsPrincipal);
        }

        public async Task<bool> DeleteConsent(string consentId)
        {
            return await _removeConsentOperation.Execute(consentId);
        }

        public async Task<Common.Models.ResourceOwner> GetUser(ClaimsPrincipal claimsPrincipal)
        {
            return await _getUserOperation.Execute(claimsPrincipal);
        }

        public async Task<bool> UpdateUser(UpdateUserParameter updateUserParameter)
        {
            return await _updateUserOperation.Execute(updateUserParameter);
        }

        public Task<bool> AddUser(AddUserParameter claimsPrincipal)
        {
            return _addUserOperation.Execute(claimsPrincipal);
        }
    }
}
