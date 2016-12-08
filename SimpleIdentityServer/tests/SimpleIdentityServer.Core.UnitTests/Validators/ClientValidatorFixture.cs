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

using Moq;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    public class ClientValidatorFixture
    {
        private IClientValidator _clientValidator;

        #region ValidateRedirectionUrl

        [Fact]
        public void When_Client_Doesnt_Contain_RedirectionUri_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeMockingObjects();

            // ACTS & ASSERTS
            Assert.Empty(_clientValidator.GetRedirectionUrls(null, null));
            Assert.Empty(_clientValidator.GetRedirectionUrls(new Models.Client(), null));
            Assert.Empty(_clientValidator.GetRedirectionUrls(new Models.Client(), "url"));
            Assert.Null(_clientValidator.GetRedirectionUrls(new Models.Client
            {
                RedirectionUrls = new List<string>()
            }, "url"));
        }

        [Fact]
        public void When_Checking_RedirectionUri_Then_Uri_Is_Returned()
        {
            // ARRANGE
            const string url = "url";
            var client = new Models.Client
            {
                RedirectionUrls = new List<string>
                {
                    url
                }
            };
            InitializeMockingObjects();

            // ACT
            var result = _clientValidator.GetRedirectionUrls(client, url);

            // ASSERT
            Assert.True(result.First() == url);
        }

        #endregion

        #region ValidateGrantType

        [Fact]
        public void When_Passing_Null_Parameter_To_ValidateGrantType_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeMockingObjects();

            // ACT
            var result = _clientValidator.CheckGrantTypes(null, GrantType.authorization_code);

            // ASSERTS
            Assert.False(result);
        }

        [Fact]
        public void When_Client_Doesnt_Have_GrantType_Then_AuthorizationCode_Is_Assigned()
        {
            // ARRANGE
            InitializeMockingObjects();
            var client = new Models.Client();

            // ACT
            var result = _clientValidator.CheckGrantTypes(client, GrantType.authorization_code);

            // ASSERTS
            Assert.True(result);
            Assert.True(client.GrantTypes.Contains(GrantType.authorization_code));
        }

        [Fact]
        public void When_Checking_Client_Has_Implicit_Grant_Type_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeMockingObjects();
            var client = new Models.Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit
                }
            };

            // ACT
            var result = _clientValidator.CheckGrantTypes(client, GrantType.@implicit);

            // ASSERTS
            Assert.True(result);
        }

        #endregion

        #region ValidateGrantTypes

        [Fact]
        public void When_Passing_Null_Parameters_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeMockingObjects();

            // ACTS & ASSERTS
            Assert.False(_clientValidator.CheckGrantTypes(null, null));
            Assert.False(_clientValidator.CheckGrantTypes(new Models.Client(), null));
        }

        [Fact]
        public void When_Checking_Client_Grant_Types_Then_True_Is_Returned()
        {
            // ARRANGE
            InitializeMockingObjects();
            var client = new Models.Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit,
                    GrantType.password
                }
            };

            // ACTS & ASSERTS
            Assert.True(_clientValidator.CheckGrantTypes(client, GrantType.@implicit, GrantType.password));
            Assert.True(_clientValidator.CheckGrantTypes(client, GrantType.@implicit));
        }

        [Fact]
        public void When_Checking_Client_Grant_Types_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeMockingObjects();
            var client = new Models.Client
            {
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit,
                    GrantType.password
                }
            };

            // ACTS & ASSERTS
            Assert.False(_clientValidator.CheckGrantTypes(client, GrantType.refresh_token));
            Assert.False(_clientValidator.CheckGrantTypes(client, GrantType.refresh_token, GrantType.password));
        }

        #endregion

        public void InitializeMockingObjects()
        {
            _clientValidator = new ClientValidator();
        }
    }
}
