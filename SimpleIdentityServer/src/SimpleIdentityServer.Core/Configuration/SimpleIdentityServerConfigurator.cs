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

namespace SimpleIdentityServer.Core.Configuration
{
    public interface ISimpleIdentityServerConfigurator
    {
        string GetIssuerName();

        double GetTokenValidityPeriodInSeconds();

        double GetAuthorizationCodeValidityPeriodInSeconds();

        string DefaultLanguage();
    }

    public class SimpleIdentityServerConfigurator : ISimpleIdentityServerConfigurator
    {
        /// <summary>
        /// Returns the issuer name.
        /// This value is used in the JWT claims.
        /// </summary>
        /// <returns>Issuer name</returns>
        public string GetIssuerName()
        {
            return "http://localhost/identity";
        }

        /// <summary>
        /// Returns the validity of an access token or identity token in seconds
        /// </summary>
        /// <returns>Validity of an access token or identity token in seconds</returns>
        public double GetTokenValidityPeriodInSeconds()
        {
            return 3000000;
        }

        /// <summary>
        /// Returns the validity period of an authorization token in seconds.
        /// </summary>
        /// <returns>Validity period is seconds</returns>
        public double GetAuthorizationCodeValidityPeriodInSeconds()
        {
            return 3000000;
        }

        public string DefaultLanguage()
        {
            return "en";
        }
    }
}
