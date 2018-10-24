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
using System;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.WebSite.User.Actions
{
    public interface IRemoveConsentOperation
    {
        Task<bool> Execute(string consentId);
    }

    internal class RemoveConsentOperation : IRemoveConsentOperation
    {
        private readonly IConsentRepository _consentRepository;
        
        public RemoveConsentOperation(IConsentRepository consentRepository)
        {
            _consentRepository = consentRepository;
        }
        
        public async Task<bool> Execute(string consentId)
        {
            if (string.IsNullOrWhiteSpace(consentId))
            {
                throw new ArgumentNullException(consentId);
            }

            var consentToBeDeleted = new Common.Models.Consent
            {
                Id = consentId
            };

            return await _consentRepository.DeleteAsync(consentToBeDeleted);
        }
    }
}
