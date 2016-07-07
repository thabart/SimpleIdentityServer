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

namespace SimpleIdentityServer.Uma.Core.Parameters
{
    public class AddClaimParameter
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }

    public class AddPolicyRuleParameter
    {
        public List<string> Scopes { get; set; }

        public List<string> ClientIdsAllowed { get; set; }

        public List<AddClaimParameter> Claims { get; set; }

        public bool IsResourceOwnerConsentNeeded { get; set; }

        public string Script { get; set; }
    }

    public class AddPolicyParameter
    {
        public List<AddPolicyRuleParameter> Rules { get; set; }

        public List<string> ResourceSetIds { get; set; }
    }
}
