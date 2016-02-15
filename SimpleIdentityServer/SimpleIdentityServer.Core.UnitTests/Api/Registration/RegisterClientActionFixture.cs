using System.Linq;
using Moq;
using NUnit.Framework;
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

namespace SimpleIdentityServer.Core.UnitTests.Api.Registration
{
    [TestFixture]
    public sealed class RegisterClientActionFixture
    {
        private Mock<IRegistrationParameterValidator> _registrationParameterValidatorFake;

        private Mock<ISimpleIdentityServerEventSource> _simpleIdentityServerEventSourceFake;

        private Mock<IJsonWebKeyConverter> _jsonWebKeyConverterFake;

        private Mock<IClientRepository> _clientRepositoryFake;

        private IRegisterClientAction _registerClientAction;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _registerClientAction.Execute(null));
        }

        [Test]
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
            Assert.IsTrue(client.ResponseTypes.Contains(ResponseType.code));
            Assert.IsTrue(client.GrantTypes.Contains(GrantType.authorization_code));
            Assert.IsTrue(client.ApplicationType == ApplicationTypes.web);
            Assert.IsTrue(client.IdTokenSignedResponseAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.IsTrue(client.IdTokenEncryptedResponseAlg == string.Empty);
            Assert.IsTrue(client.UserInfoSignedResponseAlg == Jwt.Constants.JwsAlgNames.NONE);
            Assert.IsTrue(client.UserInfoEncryptedResponseAlg == string.Empty);
            Assert.IsTrue(client.RequestObjectSigningAlg == string.Empty);
            Assert.IsTrue(client.RequestObjectEncryptionAlg == string.Empty);
            Assert.IsTrue(client.TokenEndPointAuthMethod == TokenEndPointAuthenticationMethods.client_secret_basic);
            Assert.IsTrue(client.TokenEndPointAuthSigningAlg == string.Empty);
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.OpenId));
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.Address));
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.Email));
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.Phone));
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.ProfileScope));
        }

        [Test]
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
            Assert.IsTrue(client.IdTokenEncryptedResponseEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
            
        }

        [Test]
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
            Assert.IsTrue(client.UserInfoEncryptedResponseEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
        }

        [Test]
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
            Assert.IsTrue(client.RequestObjectEncryptionEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
        }

        [Test]
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
            
            Assert.IsTrue(client.ResponseTypes.Contains(ResponseType.token));
            Assert.IsTrue(client.GrantTypes.Contains(GrantType.@implicit));
            Assert.IsTrue(client.ApplicationType == ApplicationTypes.native);
            Assert.IsTrue(client.ClientName == clientName);
            Assert.IsTrue(client.ClientUri == clientUri);
            Assert.IsTrue(client.PolicyUri == policyUri);
            Assert.IsTrue(client.TosUri == tosUri);
            Assert.IsTrue(client.JwksUri == jwksUri);
            Assert.IsNotNull(client.JsonWebKeys);
            Assert.IsTrue(client.JsonWebKeys.First().Kid == kid);
            Assert.IsTrue(client.IdTokenSignedResponseAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.IsTrue(client.IdTokenEncryptedResponseAlg == Jwt.Constants.JweAlgNames.RSA1_5);
            Assert.IsTrue(client.IdTokenEncryptedResponseEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
            Assert.IsTrue(client.UserInfoSignedResponseAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.IsTrue(client.UserInfoEncryptedResponseAlg == Jwt.Constants.JweAlgNames.RSA1_5);
            Assert.IsTrue(client.UserInfoEncryptedResponseEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
            Assert.IsTrue(client.RequestObjectSigningAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.IsTrue(client.RequestObjectEncryptionAlg == Jwt.Constants.JweAlgNames.RSA1_5);
            Assert.IsTrue(client.RequestObjectEncryptionEnc == Jwt.Constants.JweEncNames.A128CBC_HS256);
            Assert.IsTrue(client.TokenEndPointAuthMethod == TokenEndPointAuthenticationMethods.client_secret_post);
            Assert.IsTrue(client.TokenEndPointAuthSigningAlg == Jwt.Constants.JwsAlgNames.RS256);
            Assert.IsTrue(client.DefaultMaxAge == defaultMaxAge);
            Assert.IsTrue(client.DefaultAcrValues == defaultAcrValues);
            Assert.IsTrue(client.RequireAuthTime == requireAuthTime);
            Assert.IsTrue(client.InitiateLoginUri == initiateLoginUri);
            Assert.IsTrue(client.RequestUris.First() == requestUri);
            Assert.IsNotEmpty(result.ClientSecret);
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.OpenId));
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.Address));
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.Email));
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.Phone));
            Assert.IsTrue(client.AllowedScopes.Contains(Constants.StandardScopes.ProfileScope));
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
