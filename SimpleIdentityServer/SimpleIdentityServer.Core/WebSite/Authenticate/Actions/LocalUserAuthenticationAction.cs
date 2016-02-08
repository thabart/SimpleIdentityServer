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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.Logging;

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface ILocalUserAuthenticationAction
    {
        List<Claim> Execute(LocalAuthenticationParameter localAuthenticationParameter);
    }

    public sealed class LocalUserAuthenticationAction : ILocalUserAuthenticationAction
    {
        private readonly IResourceOwnerService _resourceOwnerService;

        private readonly ISimpleIdentityServerEventSource _simpleIdentityServerEventSource;

        private readonly IResourceOwnerRepository _resourceOwnerRepository;

        #region Constructor

        public LocalUserAuthenticationAction(
            IResourceOwnerService resourceOwnerService,
            ISimpleIdentityServerEventSource simpleIdentityServerEventSource,
            IResourceOwnerRepository resourceOwnerRepository)
        {
            _resourceOwnerService = resourceOwnerService;
            _simpleIdentityServerEventSource = simpleIdentityServerEventSource;
            _resourceOwnerRepository = resourceOwnerRepository;
        }

        #endregion

        #region Public methods

        public List<Claim> Execute(LocalAuthenticationParameter localAuthenticationParameter)
        {
            if (localAuthenticationParameter == null)
            {
                throw new ArgumentNullException("localAuthenticationParameter");
            }

            var subject = _resourceOwnerService.Authenticate(localAuthenticationParameter.UserName,
                localAuthenticationParameter.Password);
            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new IdentityServerAuthenticationException(ErrorDescriptions.TheResourceOwnerCredentialsAreNotCorrect);
            }

            _simpleIdentityServerEventSource.AuthenticateResourceOwner(subject);
            var resourceOwner = _resourceOwnerRepository.GetBySubject(subject);
            var claims = resourceOwner.ToClaims();
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant,
                DateTimeOffset.UtcNow.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer));
            return claims;
        }

        #endregion
    }
}
