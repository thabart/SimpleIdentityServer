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
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Manager.Core.Api.Jws.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Helpers;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jws.Actions
{
    public class CreateJwsActionFixture
    {
        private Mock<IJwsGenerator> _jwsGeneratorStub;
        private Mock<IJsonWebKeyHelper> _jsonWebKeyHelperStub;
        private ICreateJwsAction _createJwsAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var createJwsParameter = new CreateJwsParameter();
            var emptyCreateJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload()
            };

            // ACT & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJwsAction.Execute(null)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJwsAction.Execute(createJwsParameter)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJwsAction.Execute(emptyCreateJwsParameter)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_RS256Alg_But_No_Uri_And_Kid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload(),
                Alg = JwsAlg.RS256
            };
            createJwsParameter.Payload.Add("sub", "sub");

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _createJwsAction.Execute(createJwsParameter)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheJwsCannotBeGeneratedBecauseMissingParameters);
        }

        [Fact]
        public async Task When_Url_Is_Not_Well_Formed_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "invalid_url";
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload(),
                Alg = JwsAlg.RS256,
                Kid = "kid",
                Url = url
            };
            createJwsParameter.Payload.Add("sub", "sub");

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _createJwsAction.Execute(createJwsParameter)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == ErrorDescriptions.TheUrlIsNotWellFormed);
        }

        [Fact]
        public async Task When_There_Is_No_JsonWebKey_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload(),
                Alg = JwsAlg.RS256,
                Kid = kid,
                Url = url
            };
            createJwsParameter.Payload.Add("sub", "sub");
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(null));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _createJwsAction.Execute(createJwsParameter)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        [Fact]
        public async Task When_Generating_Unsigned_Jws_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            var createJwsParameter = new CreateJwsParameter
            {
                Alg = JwsAlg.none,
                Payload = new JwsPayload()
            };
            createJwsParameter.Payload.Add("sub", "sub");

            // ACT
            await _createJwsAction.Execute(createJwsParameter);

            // ASSERT
            _jwsGeneratorStub.Verify(j => j.Generate(createJwsParameter.Payload, JwsAlg.none, null));
        }

        [Fact]
        public async Task When_Generating_Signed_Jws_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJwsParameter = new CreateJwsParameter
            {
                Payload = new JwsPayload(),
                Alg = JwsAlg.RS256,
                Kid = kid,
                Url = url
            };
            var jsonWebKey = new JsonWebKey();
            createJwsParameter.Payload.Add("sub", "sub");
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(jsonWebKey));

            // ACT
            await _createJwsAction.Execute(createJwsParameter);

            // ASSERT
            _jwsGeneratorStub.Verify(j => j.Generate(createJwsParameter.Payload, JwsAlg.RS256, jsonWebKey));
        }

        private void InitializeFakeObjects()
        {
            _jwsGeneratorStub = new Mock<IJwsGenerator>();
            _jsonWebKeyHelperStub = new Mock<IJsonWebKeyHelper>();
            _createJwsAction = new CreateJwsAction(
                _jwsGeneratorStub.Object,
                _jsonWebKeyHelperStub.Object);
        }
    }
}
