﻿#region copyright
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

using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IGetConsentsOperation
    {
        Task<IEnumerable<Models.Consent>> Execute(ClaimsPrincipal claimsPrincipal);
    }

    internal class GetConsentsOperation : IGetConsentsOperation
    {
        private readonly IConsentRepository _consentRepository;

        public GetConsentsOperation(IConsentRepository consentRepository)
        {
            _consentRepository = consentRepository;
        }
        
        public async Task<IEnumerable<Models.Consent>> Execute(ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null ||
                claimsPrincipal.Identity == null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal));
            }

            var subject = claimsPrincipal.GetSubject();
            return await _consentRepository.GetConsentsForGivenUserAsync(subject).ConfigureAwait(false);
        }
    }
}
