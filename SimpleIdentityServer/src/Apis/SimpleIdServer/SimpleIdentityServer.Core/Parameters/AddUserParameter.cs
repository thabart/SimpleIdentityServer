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

using System.Collections.Generic;
using System.Security.Claims;

namespace SimpleIdentityServer.Core.Parameters
{
    public class AddUserParameter
    {
        public AddUserParameter(string login, string password)
        {
            Login = login;
            Password = password;
        }

        public AddUserParameter(string login, string password, IEnumerable<Claim> claims) : this(login, password)
        {
            Claims = claims;
        }

        public string Login { get; set; }
        public string Password { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
