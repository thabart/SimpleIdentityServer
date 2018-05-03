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
using SimpleIdentityServer.Core.Common;

namespace SimpleIdentityServer.Core.Extensions
{
    public static class AlgorithmExtensions
    {
        public static AllAlg ToAllAlg(this JwsAlg alg)
        {
            var name = Enum.GetName(typeof (JwsAlg), alg);
            return (AllAlg)Enum.Parse(typeof (AllAlg), name);
        }

        public static AllAlg ToAllAlg(this JweAlg alg)
        {
            var name = Enum.GetName(typeof(JweAlg), alg);
            return (AllAlg)Enum.Parse(typeof(AllAlg), name);
        }
    }
}
