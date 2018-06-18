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

using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface ILocalOpenIdUserAuthenticationAction
    {
        /// <summary>
        /// Authenticate local user account.
        /// Exceptions :
        /// Throw the exception <see cref="IdentityServerAuthenticationException "/> if the user cannot be authenticated
        /// </summary>
        /// <param name="localAuthenticationParameter">User's credentials</param>
        /// <param name="authorizationParameter">Authorization parameters</param>
        /// <param name="code">Encrypted & signed authorization parameters</param>
        /// <param name="claims">Returned the claims of the authenticated user</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        Task<LocalOpenIdAuthenticationResult> Execute(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code);
    }

    public class LocalOpenIdAuthenticationResult
    {
        public ActionResult ActionResult { get; set; }
        public ICollection<Claim> Claims { get; set; }
        public string TwoFactor { get; set; }
    }

    public class LocalOpenIdUserAuthenticationAction : ILocalOpenIdUserAuthenticationAction
    {
        private readonly IAuthenticateResourceOwnerService _authenticateResourceOwnerService;
        private readonly IResourceOwnerRepository _resourceOwnerRepository;
        private readonly IAuthenticateHelper _authenticateHelper;

        public LocalOpenIdUserAuthenticationAction(
            IAuthenticateResourceOwnerService authenticateResourceOwnerService,
            IResourceOwnerRepository resourceOwnerRepository,
            IAuthenticateHelper authenticateHelper)
        {
            _authenticateResourceOwnerService = authenticateResourceOwnerService;
            _resourceOwnerRepository = resourceOwnerRepository;
            _authenticateHelper = authenticateHelper;
        }

        #region Public methods

        /// <summary>
        /// Authenticate local user account.
        /// Exceptions :
        /// Throw the exception <see cref="IdentityServerAuthenticationException "/> if the user cannot be authenticated
        /// </summary>
        /// <param name="localAuthenticationParameter">User's credentials</param>
        /// <param name="authorizationParameter">Authorization parameters</param>
        /// <param name="code">Encrypted & signed authorization parameters</param>
        /// <param name="claims">Returned the claims of the authenticated user</param>
        /// <returns>Consent screen or redirect to the Index page.</returns>
        public async Task<LocalOpenIdAuthenticationResult> Execute(
            LocalAuthenticationParameter localAuthenticationParameter,
            AuthorizationParameter authorizationParameter,
            string code)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException(nameof(localAuthenticationParameter));
            }

            if (authorizationParameter == null)
            {
                throw new ArgumentNullException(nameof(authorizationParameter));
            }

            var resourceOwner = await _authenticateResourceOwnerService.AuthenticateResourceOwnerAsync(localAuthenticationParameter.UserName,
                localAuthenticationParameter.Password);
            if (resourceOwner == null)
            {
                throw new IdentityServerAuthenticationException("the resource owner credentials are not correct");
            }

            var claims = resourceOwner.Claims == null ? new List<Claim>() : resourceOwner.Claims.ToList();
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer));
            return new LocalOpenIdAuthenticationResult
            {
                ActionResult = await _authenticateHelper.ProcessRedirection(authorizationParameter,
                                code,
                                resourceOwner.Id,
                                claims),
                Claims = claims,
                TwoFactor = resourceOwner.TwoFactorAuthentication
            };
        }

        #endregion
    }
}
