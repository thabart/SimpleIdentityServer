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
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Encrypt;
using SimpleIdentityServer.Manager.Core.Api.Jwe.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Helpers;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jwe.Actions
{
    public class CreateJweActionFixture
    {
        private Mock<IJweGenerator> _jweGeneratorStub;

        private Mock<IJsonWebKeyHelper> _jsonWebKeyHelperStub;

        private ICreateJweAction _createJweAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var createJweParameterWithoutUrl = new CreateJweParameter();
            var createJweParameterWithoutJws = new CreateJweParameter
            {
                Url = "url"
            };
            var createJweParameterWithoutKid = new CreateJweParameter
            {
                Url = "url",
                Jws = "jws"
            };

            // ACT & ASSERT
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJweAction.ExecuteAsync(null)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJweAction.ExecuteAsync(createJweParameterWithoutUrl)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJweAction.ExecuteAsync(createJweParameterWithoutJws)).ConfigureAwait(false);
            Assert.ThrowsAsync<ArgumentNullException>(() => _createJweAction.ExecuteAsync(createJweParameterWithoutKid)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Not_Well_Formed_Uri_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "url";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = "kid"
            };

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _createJweAction.ExecuteAsync(createJweParameter)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
        }

        [Fact]
        public async Task When_JsonWebKey_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = kid
            };
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(null));

            // ACT & ASSERT
            var exception = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _createJweAction.ExecuteAsync(createJweParameter)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        #endregion

        #region Happy paths

        [Fact]
        public async Task When_Encrypting_Jws_With_Password_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = kid,
                Password = "password"
            };
            var jsonWebKey = new JsonWebKey();
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult(jsonWebKey));

            // ACT
            await _createJweAction.ExecuteAsync(createJweParameter).ConfigureAwait(false);

            // ASSERT
            _jweGeneratorStub.Verify(j => j.GenerateJweByUsingSymmetricPassword(It.IsAny<string>(),
                It.IsAny<JweAlg>(),
                It.IsAny<JweEnc>(),
                It.IsAny<JsonWebKey>(),
                It.IsAny<string>()));
        }

        [Fact]
        public async Task When_Encrypting_Jws_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var createJweParameter = new CreateJweParameter
            {
                Url = url,
                Jws = "jws",
                Kid = kid,
            };
            var jsonWebKey = new JsonWebKey();
            _jsonWebKeyHelperStub.Setup(j => j.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult(jsonWebKey));

            // ACT
            await _createJweAction.ExecuteAsync(createJweParameter).ConfigureAwait(false);

            // ASSERT
            _jweGeneratorStub.Verify(j => j.GenerateJwe(It.IsAny<string>(),
                It.IsAny<JweAlg>(),
                It.IsAny<JweEnc>(),
                It.IsAny<JsonWebKey>()));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _jweGeneratorStub = new Mock<IJweGenerator>();
            _jsonWebKeyHelperStub = new Mock<IJsonWebKeyHelper>();
            _createJweAction = new CreateJweAction(
                _jweGeneratorStub.Object,
                _jsonWebKeyHelperStub.Object);
        }
    }
}
