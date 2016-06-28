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

using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface ILoginCallbackAction
    {
        void Execute(ClaimsPrincipal claimsPrincipal);
    }

    internal class LoginCallbackAction : ILoginCallbackAction
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        private readonly ISecurityHelper _securityHelper;

        #region Constructor

        public LoginCallbackAction(
            IResourceOwnerRepository resourceOwnerRepository,
            ISecurityHelper securityHelper)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
            _securityHelper = securityHelper;
        }

        #endregion

        #region Public methods

        public void Execute(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            if (claimsPrincipal.Identity == null ||
                !claimsPrincipal.Identity.IsAuthenticated ||
                !(claimsPrincipal.Identity is ClaimsIdentity))
            {
                throw new IdentityServerException(
                      Errors.ErrorCodes.UnhandledExceptionCode,
                      Errors.ErrorDescriptions.TheUserNeedsToBeAuthenticated);
            }

            var subject = claimsPrincipal.GetSubject();
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheRoCannotBeCreated);
            }

            var resourceOwner = _resourceOwnerRepository.GetBySubject(subject);
            if (resourceOwner != null)
            {
                return;
            }

            var clearPassword = Guid.NewGuid().ToString();
            resourceOwner = new ResourceOwner
            {
                Id = claimsPrincipal.GetSubject(),
                Name = claimsPrincipal.GetName() ?? subject,
                Password = _securityHelper.ComputeHash(clearPassword),
                BirthDate = claimsPrincipal.GetBirthDate(),
                Email = claimsPrincipal.GetEmail(),
                EmailVerified = claimsPrincipal.GetEmailVerified(),
                FamilyName = claimsPrincipal.GetFamilyName(),
                Gender = claimsPrincipal.GetGender(),
                GivenName = claimsPrincipal.GetGivenName(),
                Locale = claimsPrincipal.GetLocale(),
                MiddleName = claimsPrincipal.GetMiddleName(),
                NickName = claimsPrincipal.GetNickName(),
                PhoneNumber = claimsPrincipal.GetPhoneNumber(),
                PhoneNumberVerified = claimsPrincipal.GetPhoneNumberVerified(),
                Picture  = claimsPrincipal.GetPicture(),
                PreferredUserName = claimsPrincipal.GetPreferredUserName(),
                Profile  = claimsPrincipal.GetProfile(),
                WebSite = claimsPrincipal.GetWebSite(),
                ZoneInfo = claimsPrincipal.GetZoneInfo(),
                UpdatedAt = DateTime.Now.ConvertToUnixTimestamp()
            };

            _resourceOwnerRepository.Insert(resourceOwner);
        }

        #endregion
    }
}
