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
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IConfirmUserOperation
    {
        void Execute(ClaimsPrincipal claimsPrincipal);
    }

    internal class ConfirmUserOperation : IConfirmUserOperation
    {
        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        #region Constructor

        public ConfirmUserOperation(IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerRepository = resourceOwnerRepository;
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
                    Errors.ErrorDescriptions.TheSubjectCannotBeRetrieved);
            }

            var result = _resourceOwnerRepository.GetBySubject(subject);
            if (result == null)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheRoDoesntExist);
            }

            if (result.IsLocalAccount)
            {
                throw new IdentityServerException(
                    Errors.ErrorCodes.UnhandledExceptionCode,
                    Errors.ErrorDescriptions.TheAccountHasAlreadyBeenActivated);
            }

            result.IsLocalAccount = true;
            _resourceOwnerRepository.Update(result);
        }

        #endregion
    }
}
