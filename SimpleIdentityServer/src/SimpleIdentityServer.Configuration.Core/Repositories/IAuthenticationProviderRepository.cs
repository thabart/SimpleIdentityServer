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

using SimpleIdentityServer.Configuration.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Configuration.Core.Repositories
{
    public interface IAuthenticationProviderRepository
    {
        Task<List<AuthenticationProvider>> GetAuthenticationProviders();

        Task<AuthenticationProvider> GetAuthenticationProvider(string name);

        Task<bool> UpdateAuthenticationProvider(AuthenticationProvider authenticationProvider);

        Task<bool> AddAuthenticationProvider(AuthenticationProvider authenticationProvider);

        Task<bool> RemoveAuthenticationProvider(string name);
    }
}
