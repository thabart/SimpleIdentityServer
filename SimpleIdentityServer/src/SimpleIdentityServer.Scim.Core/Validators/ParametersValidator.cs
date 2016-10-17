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
// limitations under the License.C:\Projects\SimpleIdentityServer\SimpleIdentityServer\src\SimpleIdentityServer.Scim.Core\DTOs\
#endregion

using SimpleIdentityServer.Scim.Core.Errors;
using System;

namespace SimpleIdentityServer.Scim.Core.Validators
{
    public interface IParametersValidator
    {
        void ValidateLocationPattern(string locationPattern);
    }

    internal class ParametersValidator : IParametersValidator
    {
        public void ValidateLocationPattern(string locationPattern)
        {
            if (string.IsNullOrWhiteSpace(locationPattern))
            {
                throw new ArgumentNullException(nameof(locationPattern));
            }

            if (!locationPattern.Contains("{id}"))
            {
                throw new ArgumentException(string.Format(ErrorMessages.TheLocationPatternIsNotCorrect, locationPattern));
            }
        }
    }
}
