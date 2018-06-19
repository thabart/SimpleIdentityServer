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

using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.TwoFactorAuthentication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Services
{
    public interface ITwoFactorAuthenticationHandler
    {
        Dictionary<string, ITwoFactorAuthenticationService> GetAll();
        ITwoFactorAuthenticationService Get(string twoFactorAuthType);
        Task SendCode(string code, string twoFactorAuthType, ResourceOwner user);
    }

    internal class TwoFactorAuthenticationHandler : ITwoFactorAuthenticationHandler
    {
        public TwoFactorAuthenticationHandler() { }

        public ITwoFactorAuthenticationService Get(string twoFactorAuthType)
        {
            if (string.IsNullOrWhiteSpace(twoFactorAuthType))
            {
                throw new ArgumentNullException(nameof(twoFactorAuthType));
            }

            return TwoFactorServiceStore.Instance().Get(twoFactorAuthType);
        }

        public Dictionary<string, ITwoFactorAuthenticationService> GetAll()
        {
            return TwoFactorServiceStore.Instance().GetAll();
        }

        public async Task SendCode(string code, string twoFactorAuthType, ResourceOwner user)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (string.IsNullOrWhiteSpace(twoFactorAuthType))
            {
                throw new ArgumentNullException(nameof(twoFactorAuthType));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }


            await TwoFactorServiceStore.Instance().Get(twoFactorAuthType).SendAsync(code, user);
        }
    }
}
