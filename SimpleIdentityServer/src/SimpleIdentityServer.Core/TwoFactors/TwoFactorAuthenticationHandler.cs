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

using SimpleIdentityServer.Client;
using SimpleIdentityServer.Configuration.Client;
using SimpleIdentityServer.Core.Configuration;
using SimpleIdentityServer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.TwoFactors
{
    public interface ITwoFactorAuthenticationHandler
    {
        Task SendCode(string code, int twoFactorAuthType, ResourceOwner user);
    }

    public interface ITwoFactorAuthenticationStore
    {
        void AddService(ITwoFactorAuthenticationService service);
    }

    internal class TwoFactorAuthenticationHandler : ITwoFactorAuthenticationHandler, ITwoFactorAuthenticationStore
    {
        private readonly List<ITwoFactorAuthenticationService> _services;

        private readonly ISimpleIdServerConfigurationClientFactory _simpleIdServerConfigurationClientFactory;

        private readonly IIdentityServerClientFactory _identityServerClientFactory;

        private readonly ISimpleIdentityServerConfigurator _simpleIdentityServerConfigurator;

        public TwoFactorAuthenticationHandler(
            ISimpleIdServerConfigurationClientFactory simpleIdServerConfigurationClientFactory,
            IIdentityServerClientFactory identityServerClientFactory,
            ISimpleIdentityServerConfigurator simpleIdentityServerConfigurator)
        {
            if (simpleIdServerConfigurationClientFactory == null)
            {
                throw new ArgumentNullException(nameof(simpleIdServerConfigurationClientFactory));
            }

            if (identityServerClientFactory == null)
            {
                throw new ArgumentNullException(nameof(identityServerClientFactory));
            }

            if (simpleIdentityServerConfigurator == null)
            {
                throw new ArgumentNullException(nameof(simpleIdentityServerConfigurator));
            }

            _simpleIdServerConfigurationClientFactory = simpleIdServerConfigurationClientFactory;
            _simpleIdentityServerConfigurator = simpleIdentityServerConfigurator;
            _services = new List<ITwoFactorAuthenticationService>
            {
                new EmailService(simpleIdServerConfigurationClientFactory, identityServerClientFactory, simpleIdentityServerConfigurator),
                new TwilioSmsService()
            };
        }

        public async Task SendCode(string code, int twoFactorAuthType, ResourceOwner user)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var service = _services.FirstOrDefault(s => s.Code == twoFactorAuthType);
            if (service == null)
            {
                throw new InvalidOperationException($"the service {twoFactorAuthType} doesn't exist");
            }

            await service.SendAsync(code, user);
        }

        public void AddService(ITwoFactorAuthenticationService service)
        {
            _services.Add(service);
        }
    }
}
