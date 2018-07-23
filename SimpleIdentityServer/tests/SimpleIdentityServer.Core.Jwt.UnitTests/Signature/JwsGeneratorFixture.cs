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
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt.Signature;
using System;
using Xunit;

namespace SimpleIdentityServer.Core.Jwt.UnitTests.Signature
{
    public sealed class JwsGeneratorFixture
    {
        private Mock<ICreateJwsSignature> _createJwsSignatureFake;

        private IJwsGenerator _jwsGenerator;

        [Fact]
        public void When_Passing_Null_Parameters_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            Assert.Throws<ArgumentNullException>(() => _jwsGenerator.Generate(null, JwsAlg.none, null));
        }

        [Fact]
        public void When_Passing_No_JsonWebKey_And_Algorithm_Value_Other_Than_None_Then_Returns_Unsigned_Result()
        {            
            // ARRANGE
            InitializeFakeObjects();
            const JwsAlg jwsAlg = JwsAlg.none;
            const KeyType keyType = KeyType.RSA;
            const string kid = "kid";
            const string serializedKey = "serializedKey";
            const string signature = "signature";

            var jsonWebKey = new JsonWebKey
            {
                Kty = keyType,
                Kid = kid,
                SerializedKey = serializedKey
            };
            var jwsPayload = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Enum.GetName(typeof(JwsAlg), jwsAlg),
                Type = "JWT"
            };
            _createJwsSignatureFake.Setup(c => c.SignWithRsa(It.IsAny<JwsAlg>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(signature);
            var serializedJwsProtectedHeader = jwsProtectedHeader.SerializeWithDataContract();
            var base64SerializedJwsProtectedHeader = serializedJwsProtectedHeader.Base64Encode();
            var serializedJwsPayload = jwsPayload.SerializeWithJavascript();
            var base64SerializedJwsPayload = serializedJwsPayload.Base64Encode();
            var combined = string.Format("{0}.{1}",
                base64SerializedJwsProtectedHeader,
                base64SerializedJwsPayload);
            var expectedResult = string.Format("{0}.{1}",
                combined,
                string.Empty);

            // ACT
            var result = _jwsGenerator.Generate(jwsPayload, jwsAlg, null);

            // ASSERT
            _createJwsSignatureFake.Verify(c => c.SignWithRsa(It.IsAny<JwsAlg>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.True(expectedResult == result);
        }

        [Fact]
        public void When_Sign_Payload_With_Rsa_Alogirthm_Then_Jws_Token_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const KeyType keyType = KeyType.RSA;
            const string kid = "kid";
            const JwsAlg jwsAlg = JwsAlg.RS384;
            const string serializedKey = "serializedKey";
            const string signature = "signature";
            
            var jsonWebKey = new JsonWebKey
            {
                Kty = keyType,
                Kid = kid,
                SerializedKey = serializedKey
            };
            var jwsPayload = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Kid = kid,
                Alg = Enum.GetName(typeof(JwsAlg), jwsAlg),
                Type = "JWT"
            };
            _createJwsSignatureFake.Setup(c => c.SignWithRsa(It.IsAny<JwsAlg>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(signature);
            var serializedJwsProtectedHeader = jwsProtectedHeader.SerializeWithDataContract();
            var base64SerializedJwsProtectedHeader = serializedJwsProtectedHeader.Base64Encode();
            var serializedJwsPayload = jwsPayload.SerializeWithJavascript();
            var base64SerializedJwsPayload = serializedJwsPayload.Base64Encode();
            var combined = string.Format("{0}.{1}",
                base64SerializedJwsProtectedHeader,
                base64SerializedJwsPayload);
            var expectedResult = string.Format("{0}.{1}",
                combined,
                signature);

            // ACT
            var result = _jwsGenerator.Generate(jwsPayload, jwsAlg, jsonWebKey);
            
            // ASSERT
            _createJwsSignatureFake.Verify(c => c.SignWithRsa(jwsAlg, serializedKey, combined));
            Assert.True(expectedResult == result);
        }

        [Fact]
        public void When_Sign_Payload_With_None_Alogithm_Then_Jws_Token_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const JwsAlg jwsAlg = JwsAlg.none;
            const KeyType keyType = KeyType.RSA;
            const string kid = "kid";
            const string serializedKey = "serializedKey";
            const string signature = "signature";

            var jsonWebKey = new JsonWebKey
            {
                Kty = keyType,
                Kid = kid,
                SerializedKey = serializedKey
            };
            var jwsPayload = new JwsPayload();
            var jwsProtectedHeader = new JwsProtectedHeader
            {
                Alg = Enum.GetName(typeof(JwsAlg), jwsAlg),
                Type = "JWT"
            };
            _createJwsSignatureFake.Setup(c => c.SignWithRsa(It.IsAny<JwsAlg>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .Returns(signature);
            var serializedJwsProtectedHeader = jwsProtectedHeader.SerializeWithDataContract();
            var base64SerializedJwsProtectedHeader = serializedJwsProtectedHeader.Base64Encode();
            var serializedJwsPayload = jwsPayload.SerializeWithJavascript();
            var base64SerializedJwsPayload = serializedJwsPayload.Base64Encode();
            var combined = string.Format("{0}.{1}",
                base64SerializedJwsProtectedHeader,
                base64SerializedJwsPayload);
            var expectedResult = string.Format("{0}.{1}",
                combined,
                string.Empty);

            // ACT
            var result = _jwsGenerator.Generate(jwsPayload, jwsAlg, jsonWebKey);

            // ASSERT
            _createJwsSignatureFake.Verify(c => c.SignWithRsa(It.IsAny<JwsAlg>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.True(expectedResult == result);
        }

        private void InitializeFakeObjects()
        {
            _createJwsSignatureFake = new Mock<ICreateJwsSignature>();
            _jwsGenerator = new JwsGenerator(_createJwsSignatureFake.Object);
        }
    }
}
