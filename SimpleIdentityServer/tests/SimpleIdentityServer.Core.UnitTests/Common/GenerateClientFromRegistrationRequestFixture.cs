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
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Common
{
    public class GenerateClientFromRegistrationRequestFixture
    {
        private Mock<IRegistrationParameterValidator> _registrationParameterValidatorFake;
        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterFake;
        private IGenerateClientFromRegistrationRequest _generateClientFromRegistrationRequest;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _generateClientFromRegistrationRequest.Execute(null));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Passing_Registration_Parameter_Without_Specific_Values_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientName = "client_name";
            var registrationParameter = new RegistrationParameter
            {
                ClientName = clientName
            };

            // ACT
            var client = _generateClientFromRegistrationRequest.Execute(registrationParameter);

            // ASSERT
            _registrationParameterValidatorFake.Verify(r => r.Validate(registrationParameter));
            Assert.True(client.ResponseTypes.Contains(ResponseType.code));
            Assert.True(client.GrantTypes.Contains(GrantType.authorization_code));
            Assert.True(client.ApplicationType == ApplicationTypes.web);
            Assert.True(client.IdTokenSignedResponseAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.True(client.IdTokenEncryptedResponseAlg == string.Empty);
            Assert.True(client.UserInfoSignedResponseAlg == Jwt.Constants.JwsAlgNames.NONE);
            Assert.True(client.UserInfoEncryptedResponseAlg == string.Empty);
            Assert.True(client.RequestObjectSigningAlg == string.Empty);
            Assert.True(client.RequestObjectEncryptionAlg == string.Empty);
            Assert.True(client.TokenEndPointAuthMethod == TokenEndPointAuthenticationMethods.client_secret_basic);
            Assert.True(client.TokenEndPointAuthSigningAlg == string.Empty);
        }

        [Fact]
        public void When_Passing_Registration_Parameter_With_A_Not_Supported_IdTokenEncryptedResponseEnc_Then_Default_Value_Is_A128CBC_HS256()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientName = "client_name";
            var registrationParameter = new RegistrationParameter
            {
                ClientName = clientName,
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var client = _generateClientFromRegistrationRequest.Execute(registrationParameter);

            // ASSERT
            Assert.True(client.IdTokenEncryptedResponseEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);

        }

        [Fact]
        public void When_Passing_Registration_Parameter_With_A_Not_Supported_UserInfoEncryptedResponseEnc_Then_Default_Value_Is_A128CBC_HS256()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientName = "client_name";
            var registrationParameter = new RegistrationParameter
            {
                ClientName = clientName,
                UserInfoEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var client = _generateClientFromRegistrationRequest.Execute(registrationParameter);

            // ASSERT
            Assert.True(client.UserInfoEncryptedResponseEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
        }

        [Fact]
        public void When_Passing_Registration_Parameter_With_A_Not_Supported_RequestObjectEncryptionEnc_Then_Default_Value_Is_A128CBC_HS256()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientName = "client_name";
            var registrationParameter = new RegistrationParameter
            {
                ClientName = clientName,
                RequestObjectEncryptionAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var client = _generateClientFromRegistrationRequest.Execute(registrationParameter);

            // ASSERT
            Assert.True(client.RequestObjectEncryptionEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
        }

        [Fact]
        public void When_Passing_Registration_Parameter_With_Specific_Values_Then_Client_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string clientName = "client_name";
            const string clientUri = "client_uri";
            const string policyUri = "policy_uri";
            const string tosUri = "tos_uri";
            const string jwksUri = "jwks_uri";
            const string kid = "kid";
            const string sectorIdentifierUri = "sector_identifier_uri";
            const double defaultMaxAge = 3;
            const string defaultAcrValues = "default_acr_values";
            const bool requireAuthTime = false;
            const string initiateLoginUri = "initiate_login_uri";
            const string requestUri = "request_uri";
            var registrationParameter = new RegistrationParameter
            {
                ClientName = clientName,
                ResponseTypes = new List<ResponseType>
                {
                    ResponseType.token
                },
                GrantTypes = new List<GrantType>
                {
                    GrantType.@implicit
                },
                ApplicationType = ApplicationTypes.native,
                ClientUri = clientUri,
                PolicyUri = policyUri,
                TosUri = tosUri,
                JwksUri = jwksUri,
                Jwks = new JsonWebKeySet(),
                SectorIdentifierUri = sectorIdentifierUri,
                IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256,
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5,
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                UserInfoSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256,
                UserInfoEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5,
                UserInfoEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                RequestObjectSigningAlg = Jwt.Constants.JwsAlgNames.RS256,
                RequestObjectEncryptionAlg = Jwt.Constants.JweAlgNames.RSA1_5,
                RequestObjectEncryptionEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                TokenEndPointAuthMethod = "client_secret_post",
                TokenEndPointAuthSigningAlg = Jwt.Constants.JwsAlgNames.RS256,
                DefaultMaxAge = defaultMaxAge,
                DefaultAcrValues = defaultAcrValues,
                RequireAuthTime = requireAuthTime,
                InitiateLoginUri = initiateLoginUri,
                RequestUris = new List<string>
                {
                    requestUri
                }
            };
            var jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Kid = kid
                }
            };
            _jsonWebKeyConverterFake.Setup(j => j.ExtractSerializedKeys(It.IsAny<JsonWebKeySet>()))
                .Returns(jsonWebKeys);

            // ACT
            var client = _generateClientFromRegistrationRequest.Execute(registrationParameter);

            // ASSERT
            _registrationParameterValidatorFake.Verify(r => r.Validate(registrationParameter));

            Assert.True(client.ResponseTypes.Contains(ResponseType.token));
            Assert.True(client.GrantTypes.Contains(GrantType.@implicit));
            Assert.True(client.ApplicationType == ApplicationTypes.native);
            Assert.True(client.ClientName == clientName);
            Assert.True(client.ClientUri == clientUri);
            Assert.True(client.PolicyUri == policyUri);
            Assert.True(client.TosUri == tosUri);
            Assert.True(client.JwksUri == jwksUri);
            Assert.NotNull(client.JsonWebKeys);
            Assert.True(client.JsonWebKeys.First().Kid == kid);
            Assert.True(client.IdTokenSignedResponseAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.True(client.IdTokenEncryptedResponseAlg == Jwt.Constants.JweAlgNames.RSA1_5);
            Assert.True(client.IdTokenEncryptedResponseEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
            Assert.True(client.UserInfoSignedResponseAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.True(client.UserInfoEncryptedResponseAlg == Jwt.Constants.JweAlgNames.RSA1_5);
            Assert.True(client.UserInfoEncryptedResponseEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
            Assert.True(client.RequestObjectSigningAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.True(client.RequestObjectEncryptionAlg == Jwt.Constants.JweAlgNames.RSA1_5);
            Assert.True(client.RequestObjectEncryptionEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
            Assert.True(client.TokenEndPointAuthMethod == TokenEndPointAuthenticationMethods.client_secret_post);
            Assert.True(client.TokenEndPointAuthSigningAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.True(client.DefaultMaxAge == defaultMaxAge);
            Assert.True(client.DefaultAcrValues == defaultAcrValues);
            Assert.True(client.RequireAuthTime == requireAuthTime);
            Assert.True(client.InitiateLoginUri == initiateLoginUri);
            Assert.True(client.RequestUris.First() == requestUri);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _registrationParameterValidatorFake = new Mock<IRegistrationParameterValidator>();
            _jsonWebKeyConverterFake = new Mock<IJsonWebKeyConverter>();
            _generateClientFromRegistrationRequest = new GenerateClientFromRegistrationRequest(
                _registrationParameterValidatorFake.Object,
                _jsonWebKeyConverterFake.Object);
        }
    }
}
