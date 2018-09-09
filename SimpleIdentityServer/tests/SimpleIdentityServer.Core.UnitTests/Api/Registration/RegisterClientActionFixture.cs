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
using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Common;
using SimpleIdentityServer.Core.Common.DTOs.Requests;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Services;
using SimpleIdentityServer.OAuth.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Registration
{
    public sealed class RegisterClientActionFixture
    {
        private Mock<IOAuthEventSource> _oauthEventSource;
        private Mock<IClientRepository> _clientRepositoryFake;
        private Mock<IGenerateClientFromRegistrationRequest> _generateClientFromRegistrationRequest;
        private Mock<IPasswordService> _encryptedPasswordFactoryStub;
        private IRegisterClientAction _registerClientAction;

        #region Exceptions

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _registerClientAction.Execute(null)).ConfigureAwait(false);
        }

        #endregion

        #region Happy Path
        
        [Fact]
        public async Task When_Passing_Registration_Parameter_With_Specific_Values_Then_Client_Is_Returned()
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
            var client = new Core.Common.Models.Client
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
                JsonWebKeys = new List<JsonWebKey>(),
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
                TokenEndPointAuthMethod = TokenEndPointAuthenticationMethods.client_secret_basic,
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
            _generateClientFromRegistrationRequest.Setup(g => g.Execute(It.IsAny<RegistrationParameter>()))
                .Returns(client);                
            _clientRepositoryFake.Setup(c => c.InsertAsync(It.IsAny<Core.Common.Models.Client>()))
                .Callback<Core.Common.Models.Client>(c => client = c)
                .Returns(Task.FromResult(true));

            // ACT
            var result = await _registerClientAction.Execute(registrationParameter).ConfigureAwait(false);

            // ASSERT
            _oauthEventSource.Verify(s => s.StartRegistration(clientName));
            _clientRepositoryFake.Verify(c => c.InsertAsync(It.IsAny<Core.Common.Models.Client>()));
            _oauthEventSource.Verify(s => s.EndRegistration(It.IsAny<string>(), clientName));
            Assert.NotEmpty(result.ClientSecret);
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.OpenId));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Address));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Email));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Phone));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.ProfileScope));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _oauthEventSource = new Mock<IOAuthEventSource>();
            _clientRepositoryFake = new Mock<IClientRepository>();
            _generateClientFromRegistrationRequest = new Mock<IGenerateClientFromRegistrationRequest>();
            _encryptedPasswordFactoryStub = new Mock<IPasswordService>();
            _registerClientAction = new RegisterClientAction(
                _oauthEventSource.Object,
                _clientRepositoryFake.Object,
                _generateClientFromRegistrationRequest.Object,
                _encryptedPasswordFactoryStub.Object);
        }
    }
}
