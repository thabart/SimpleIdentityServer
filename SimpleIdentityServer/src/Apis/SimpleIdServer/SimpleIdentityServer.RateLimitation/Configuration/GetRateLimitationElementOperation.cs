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

using Microsoft.Extensions.Options;

namespace SimpleIdentityServer.RateLimitation.Configuration
{
    public interface IGetRateLimitationElementOperation
    {
        RateLimitationElement Execute(string rateLimitationElementName);

        bool IsEnabled();
    }

    public class GetRateLimitationElementOperation : IGetRateLimitationElementOperation
    {
        private RateLimitationOptions _rateLimitationOptions;

        #region Constructor

        public GetRateLimitationElementOperation(IOptions<RateLimitationOptions> rateLimitationOptions)
        {
            _rateLimitationOptions = rateLimitationOptions.Value;
        }

        #endregion

        #region Public methods

        public RateLimitationElement Execute(string rateLimitationElementName)
        {
            if (_rateLimitationOptions == null)
            {
                return null;
            }

            foreach (RateLimitationElement rateLimitation in _rateLimitationOptions.RateLimitationElements)
            {
                if (rateLimitation.Name.Equals(rateLimitationElementName))
                {
                    return rateLimitation;
                }
            }

            return null;
        }

        public bool IsEnabled()
        {
            if (_rateLimitationOptions == null)
            {
                return true;
            }

            return _rateLimitationOptions.IsEnabled;
        }

        #endregion
    }
}
