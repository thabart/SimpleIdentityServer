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

namespace SimpleIdentityServer.Core.WebSite.Authenticate.Actions
{
    public interface IRemoveConfirmationCodeAction
    {
        Task<bool> Execute(string code);
    }

    internal class RemoveConfirmationCodeAction : IRemoveConfirmationCodeAction
    {
        private readonly IConfirmationCodeRepository _confirmationCodeRepository;

        public RemoveConfirmationCodeAction(IConfirmationCodeRepository confirmationCodeRepository)
        {
            _confirmationCodeRepository = confirmationCodeRepository;
        }

        public async Task<bool> Execute(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentNullException(nameof(code));
            }

            return await _confirmationCodeRepository.RemoveAsync(code);
        }
    }
}
