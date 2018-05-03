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
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Helpers;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Results;
using SimpleIdentityServer.Core.WebSite.Authenticate.Actions;
using SimpleIdentityServer.Core.WebSite.Authenticate.Common;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.Authenticate
{
    public sealed class AuthenticateResourceOwnerOpenIdActionFixture
    {
        private Mock<IParameterParserHelper> _parameterParserHelperFake;
        private Mock<IActionResultFactory> _actionResultFactoryFake;
        private Mock<IAuthenticateHelper> _authenticateHelperFake;
        private IAuthenticateResourceOwnerOpenIdAction _authenticateResourceOwnerOpenIdAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _authenticateResourceOwnerOpenIdAction.Execute(null, null, null));
        }

        [Fact]
        public async Task When_No_Resource_Owner_Is_Passed_Then_Redirect_To_Index_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();

            // ACT
            await _authenticateResourceOwnerOpenIdAction.Execute(authorizationParameter, null, null);

            // ASSERT
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithNoEffect());
        }

        [Fact]
        public async Task When_Resource_Owner_Is_Not_Authenticated_Then_Redirect_To_Index_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();
            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            await _authenticateResourceOwnerOpenIdAction.Execute(authorizationParameter, 
                claimsPrincipal, 
                null);

            // ASSERT
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithNoEffect());
        }

        [Fact]
        public async Task When_Prompt_Parameter_Contains_Login_Value_Then_Redirect_To_Index_Page()
        {
            // ARRANGE
            InitializeFakeObjects();
            var authorizationParameter = new AuthorizationParameter();
            var claimsIdentity = new ClaimsIdentity("identityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var promptParameters = new List<PromptParameter>
            {
                PromptParameter.login
            };
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(promptParameters);

            // ACT
            await _authenticateResourceOwnerOpenIdAction.Execute(authorizationParameter,
                claimsPrincipal,
                null);

            // ASSERT
            _actionResultFactoryFake.Verify(a => a.CreateAnEmptyActionResultWithNoEffect());
        }

        [Fact]
        public async Task When_Prompt_Parameter_Doesnt_Contain_Login_Value_And_Resource_Owner_Is_Authenticated_Then_Helper_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string code = "code";
            const string subject = "subject";
            var authorizationParameter = new AuthorizationParameter();
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "identityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var promptParameters = new List<PromptParameter>
            {
                PromptParameter.consent
            };
            var actionResult = new ActionResult
            {
                RedirectInstruction = new RedirectInstruction()
            };
            _parameterParserHelperFake.Setup(p => p.ParsePrompts(It.IsAny<string>()))
                .Returns(promptParameters);
            _actionResultFactoryFake.Setup(a => a.CreateAnEmptyActionResultWithRedirection())
                .Returns(actionResult);

            // ACT
            await _authenticateResourceOwnerOpenIdAction.Execute(authorizationParameter,
                claimsPrincipal,
                code);

            // ASSERT
            _authenticateHelperFake.Verify(a => a.ProcessRedirection(authorizationParameter, 
                code, 
                subject,
                It.IsAny<List<Claim>>()));
        }

        private void InitializeFakeObjects()
        {
            _parameterParserHelperFake = new Mock<IParameterParserHelper>();
            _actionResultFactoryFake = new Mock<IActionResultFactory>();
            _authenticateHelperFake = new Mock<IAuthenticateHelper>();
            _authenticateResourceOwnerOpenIdAction = new AuthenticateResourceOwnerOpenIdAction(
                _parameterParserHelperFake.Object,
                _actionResultFactoryFake.Object,
                _authenticateHelperFake.Object);
        }
    }
}
