#region copyright
// Copyright 2016 Habart Thierry
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

namespace SimpleIdentityServer.Rfid.Website.ViewModels
{
    public class ConsentViewModel
    {
        public string Id { get; set; }
        public string ClientDisplayName { get; set; }
        public List<string> AllowedScopeDescriptions { get; set; }
        public List<string> AllowedIndividualClaims { get; set; }
        public string LogoUri { get; set; }
        public string PolicyUri { get; set; }
        public string TosUri { get; set; }
        public string Code { get; set; }
    }
}
