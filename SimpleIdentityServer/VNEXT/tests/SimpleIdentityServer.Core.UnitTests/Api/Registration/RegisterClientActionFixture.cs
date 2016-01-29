using System.Linq;
using Moq;
using SimpleIdentityServer.Core.Api.Registration.Actions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Models;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Repositories;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Logging;
using System;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Registration
{
    public sealed class RegisterClientActionFixture
    {
        private Mock<IRegistrationParameterValidator> _registrationParameterValidatorFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;

        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterFake;

        private Mock<IClientRepository> _clientRepositoryFake;

        private IRegisterClientAction _registerClientAction;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _registerClientAction.Execute(null));
        }

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
            var client = new Client();
            _clientRepositoryFake.Setup(c => c.InsertClient(It.IsAny<Client>()))
                .Callback<Client>(c => client = c);

            // ACT
            _registerClientAction.Execute(registrationParameter);

            // ASSERT
            _registrationParameterValidatorFake.Verify(r => r.Validate(registrationParameter));
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartRegistration(clientName));
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
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.OpenId));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Address));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Email));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Phone));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.ProfileScope));
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
            var client = new Client();
            _clientRepositoryFake.Setup(c => c.InsertClient(It.IsAny<Client>()))
                .Callback<Client>(c => client = c);

            // ACT
            _registerClientAction.Execute(registrationParameter);

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
            var client = new Client();
            _clientRepositoryFake.Setup(c => c.InsertClient(It.IsAny<Client>()))
                .Callback<Client>(c => client = c);

            // ACT
            _registerClientAction.Execute(registrationParameter);

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
            var client = new Client();
            _clientRepositoryFake.Setup(c => c.InsertClient(It.IsAny<Client>()))
                .Callback<Client>(c => client = c);

            // ACT
            _registerClientAction.Execute(registrationParameter);

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
            var client = new Client();
            _clientRepositoryFake.Setup(c => c.InsertClient(It.IsAny<Client>()))
                .Callback<Client>(c => client = c);

            // ACT
            var result = _registerClientAction.Execute(registrationParameter);

            // ASSERT
            _registrationParameterValidatorFake.Verify(r => r.Validate(registrationParameter));
            _simpleIdentityServerEventSourceFake.Verify(s => s.StartRegistration(clientName));
            _clientRepositoryFake.Verify(c => c.InsertClient(It.IsAny<Client>()));
            _simpleIdentityServerEventSourceFake.Verify(s => s.EndRegistration(It.IsAny<string>(), clientName));
            
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
            Assert.NotEmpty(result.ClientSecret);
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.OpenId));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Address));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Email));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.Phone));
            Assert.True(client.AllowedScopes.Contains(Constants.StandardScopes.ProfileScope));
        }

        private void InitializeFakeObjects()
        {
            _registrationParameterValidatorFake = new Mock<IRegistrationParameterValidator>();
            _simpleIdentityServerEventSourceFake = new Mock<ISimpleIdentityServerEventSource>();
            _jsonWebKeyConverterFake = new Mock<IJsonWebKeyConverter>();
            _clientRepositoryFake = new Mock<IClientRepository>();
            _registerClientAction = new RegisterClientAction(
                _registrationParameterValidatorFake.Object,
                _simpleIdentityServerEventSourceFake.Object,
                _jsonWebKeyConverterFake.Object,
                _clientRepositoryFake.Object);
        }
    }
}
