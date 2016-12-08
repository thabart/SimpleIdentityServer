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

using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;
using System.Threading.Tasks;

namespace SimpleIdentityServer.Core.Repositories
{
    public interface IGrantedTokenRepository
    {
        Task<bool> InsertAsync(GrantedToken grantedToken);
        Task<bool> DeleteAsync(GrantedToken grantedToken);
        Task<bool> UpdateAsync(GrantedToken grantedToken);
        Task<GrantedToken> GetTokenAsync(string scopes, string clientId, JwsPayload idTokenJwsPayload, JwsPayload userInfoJwsPayload);
        Task<GrantedToken> GetTokenByRefreshTokenAsync(string refreshToken);
        Task<GrantedToken> GetTokenAsync(string accessToken);
    }
}
