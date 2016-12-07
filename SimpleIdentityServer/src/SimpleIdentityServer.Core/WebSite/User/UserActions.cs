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

using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.WebSite.User
{
    public interface IUserActions
    {
        IEnumerable<Models.Consent> GetConsents(ClaimsPrincipal claimsPrincipal);
        bool DeleteConsent(string consentId);
        Models.ResourceOwner GetUser(ClaimsPrincipal claimsPrincipal);
        void UpdateUser(UpdateUserParameter updateUserParameter);
        void ConfirmUser(ClaimsPrincipal claimsPrincipal);
    }

    internal class UserActions : IUserActions
    {
        private readonly IGetConsentsOperation _getConsentsOperation;
        private readonly IRemoveConsentOperation _removeConsentOperation;
        private readonly IGetUserOperation _getUserOperation;
        private readonly IUpdateUserOperation _updateUserOperation;
        private readonly IConfirmUserOperation _confirmUserOperation;

        #region Constructor

        public UserActions(
            IGetConsentsOperation getConsentsOperation,
            IRemoveConsentOperation removeConsentOperation,
            IGetUserOperation getUserOperation,
            IUpdateUserOperation updateUserOperation,
            IConfirmUserOperation confirmUserOperation)
        {
            _getConsentsOperation = getConsentsOperation;
            _removeConsentOperation = removeConsentOperation;
            _getUserOperation = getUserOperation;
            _updateUserOperation = updateUserOperation;
            _confirmUserOperation = confirmUserOperation;
        }

        #endregion

        #region Public methods

        public IEnumerable<Models.Consent> GetConsents(ClaimsPrincipal claimsPrincipal)
        {
            return _getConsentsOperation.Execute(claimsPrincipal);
        }

        public bool DeleteConsent(string consentId)
        {
            return _removeConsentOperation.Execute(consentId);
        }

        public Models.ResourceOwner GetUser(ClaimsPrincipal claimsPrincipal)
        {
            return _getUserOperation.Execute(claimsPrincipal);
        }

        public void UpdateUser(UpdateUserParameter updateUserParameter)
        {
            _updateUserOperation.Execute(updateUserParameter);
        }

        public void ConfirmUser(ClaimsPrincipal claimsPrincipal)
        {
            _confirmUserOperation.Execute(claimsPrincipal);
        }

        #endregion
    }
}
