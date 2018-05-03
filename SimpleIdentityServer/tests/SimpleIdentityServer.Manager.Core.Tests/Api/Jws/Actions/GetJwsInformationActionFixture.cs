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
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Manager.Core.Api.Jws.Actions;
using SimpleIdentityServer.Manager.Core.Errors;
using SimpleIdentityServer.Manager.Core.Exceptions;
using SimpleIdentityServer.Manager.Core.Helpers;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jws.Actions
{
    public class GetJwsInformationActionFixture
    {
        private Mock<IJwsParser> _jwsParserStub;
        private Mock<IJsonWebKeyHelper> _jsonWebKeyHelperStub;
        private Mock<IJsonWebKeyEnricher> _jsonWebKeyEnricherStub;
        private IGetJwsInformationAction _getJwsInformationAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<AggregateException>(() => _getJwsInformationAction.Execute(null)).ConfigureAwait(false);
            Assert.ThrowsAsync<AggregateException>(() => _getJwsInformationAction.Execute(getJwsParameter)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_Passing_Not_Well_Formed_Url_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "not_well_formed";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };

            // ACTS & ASSERTS
            var innerException = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _getJwsInformationAction.Execute(getJwsParameter)).ConfigureAwait(false);
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == string.Format(ErrorDescriptions.TheUrlIsNotWellFormed, url));
        }

        [Fact]
        public async Task When_Passing_A_Not_Valid_Jws_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Url = "http://google.be",
                Jws = "jws"
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var innerException = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _getJwsInformationAction.Execute(getJwsParameter)).ConfigureAwait(false);
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheTokenIsNotAValidJws);
        }

        [Fact]
        public async Task When_No_Uri_And_Sign_Alg_Are_Specified_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = "kid",
                Alg = Constants.JwsAlgNames.RS256
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);

            // ACT & ASSERTS
            var innerException = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _getJwsInformationAction.Execute(getJwsParameter)).ConfigureAwait(false);
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheSignatureCannotBeChecked);
        }

        [Fact]
        public async Task When_JsonWebKey_Doesnt_Exist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid,
                Alg = Constants.JwsAlgNames.RS256
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyHelperStub.Setup(h => h.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult<JsonWebKey>(null));

            // ACT & ASSERTS
            var innerException = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _getJwsInformationAction.Execute(getJwsParameter)).ConfigureAwait(false);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == string.Format(ErrorDescriptions.TheJsonWebKeyCannotBeFound, kid, url));
        }

        [Fact]
        public async Task When_The_Signature_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithJavascript();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid
            };
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid
            };
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyHelperStub.Setup(h => h.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult(jsonWebKey));
            _jwsParserStub.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(() => null);

            // ACT & ASSERTS
            var innerException = await Assert.ThrowsAsync<IdentityServerManagerException>(async () => await _getJwsInformationAction.Execute(getJwsParameter)).ConfigureAwait(false);
            Assert.NotNull(innerException);
            Assert.True(innerException.Code == ErrorCodes.InvalidRequestCode);
            Assert.True(innerException.Message == ErrorDescriptions.TheSignatureIsNotCorrect);
        }

        [Fact]
        public async Task When_JsonWebKey_Is_Extracted_And_The_Jws_Is_Unsigned_Then_Information_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string url = "http://google.be/";
            const string kid = "kid";
            var getJwsParameter = new GetJwsParameter
            {
                Url = url,
                Jws = "jws"
            };
            var jsonWebKeySet = new JsonWebKeySet();
            var json = jsonWebKeySet.SerializeWithJavascript();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid
            };
            var jsonWebKey = new JsonWebKey
            {
                Kid = kid
            };
            var dic = new Dictionary<string, object>
            {
                {
                    "kid", kid
                }
            };
            var jwsPayload = new JwsPayload();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jsonWebKeyHelperStub.Setup(h => h.GetJsonWebKey(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(Task.FromResult(jsonWebKey));
            _jwsParserStub.Setup(j => j.ValidateSignature(It.IsAny<string>(), It.IsAny<JsonWebKey>()))
                .Returns(jwsPayload);
            _jsonWebKeyEnricherStub.Setup(j => j.GetJsonWebKeyInformation(It.IsAny<JsonWebKey>()))
                .Returns(dic);
            _jsonWebKeyEnricherStub.Setup(j => j.GetPublicKeyInformation(It.IsAny<JsonWebKey>()))
                .Returns(() => new Dictionary<string, object>());

            // ACT
            var result = await _getJwsInformationAction.Execute(getJwsParameter);

            // ASSERTS
            Assert.NotNull(result);
            Assert.True(result.JsonWebKey.ContainsKey("kid"));
            Assert.True(result.JsonWebKey.First().Value == kid);
        }

        [Fact]
        public async Task When_Extracting_Information_Of_Unsigned_Jws_Then_Information_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws"
            };
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = "kid",
                Alg = Constants.JwsAlgNames.NONE
            };
            var jwsPayload = new JwsPayload();
            _jwsParserStub.Setup(j => j.GetHeader(It.IsAny<string>()))
                .Returns(jwsProtectedHeader);
            _jwsParserStub.Setup(j => j.GetPayload(It.IsAny<string>()))
                .Returns(jwsPayload);

            // ACT
            var result = await _getJwsInformationAction.Execute(getJwsParameter);

            // ASSERTS
            Assert.NotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _jwsParserStub = new Mock<IJwsParser>();
            _jsonWebKeyHelperStub = new Mock<IJsonWebKeyHelper>();
            _jsonWebKeyEnricherStub = new Mock<IJsonWebKeyEnricher>();
            _getJwsInformationAction = new GetJwsInformationAction(
                _jwsParserStub.Object,
                _jsonWebKeyHelperStub.Object,
                _jsonWebKeyEnricherStub.Object);
        }
    }
}
