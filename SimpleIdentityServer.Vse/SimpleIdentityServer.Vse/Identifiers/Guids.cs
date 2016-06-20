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

using System;

namespace SimpleIdentityServer.Vse.Identifiers
{
    public static class Guids
    {
        public const string PackageStr = "a67a9bce-d4bd-4ea3-9306-f26d6355edf1";
        public static Guid Package = new Guid(PackageStr);
        public static Guid MyMenuGroup = new Guid("887c8c76-0121-48b1-8bbf-61e5b84821a5");
    }
}
