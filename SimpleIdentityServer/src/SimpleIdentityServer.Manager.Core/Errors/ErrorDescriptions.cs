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

namespace SimpleIdentityServer.Manager.Core.Errors
{
    public static class ErrorDescriptions
    {
        public const string TheUrlIsNotWellFormed = "the url {0} is not well formed";

        public const string TheTokenIsNotAValidJws = "the token is not a valid JWS";

        public const string TheJsonWebKeyCannotBeFound = "the json web key {0} cannot be found {1}";

        public const string TheSignatureIsNotCorrect = "the signature is not correct";

        public const string TheSignatureCannotBeChecked = "the signature cannot be checked if the URI is not specified";
    }
}
