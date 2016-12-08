using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Moq;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Factories;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.UnitTests.Fake;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Models;
using Xunit;
using SimpleIdentityServer.Core.Common.DTOs;
using SimpleIdentityServer.Core.Common;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    public sealed class RegistrationParameterValidatorFixture
    {
        private Mock<IHttpClientFactory> _httpClientFactoryFake;
        private IRegistrationParameterValidator _registrationParameterValidator;

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _registrationParameterValidator.Validate(null));
        }

        [Fact]
        public void When_There_Is_No_Request_Uri_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = null
            };
            
            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.MissingParameter, ClientNames.RequestUris));
        }

        [Fact]
        public void When_One_Request_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "invalid"
                }
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.True(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        [Fact]
        public void When_One_Request_Uri_Contains_A_Fragment_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "http://localhost#localhost"
                }
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.True(ex.Message == ErrorDescriptions.TheRedirectUriContainsAFragment);
        }

        [Fact]
        public void When_ResponseType_Is_Not_Defined_Then_Set_To_Code()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                }
            };

            // ACT
            _registrationParameterValidator.Validate(parameter);

            // ASSERT
            Assert.NotNull(parameter);
            Assert.True(parameter.ResponseTypes.Count == 1);
            Assert.True(parameter.ResponseTypes.Contains(Core.Models.ResponseType.code));
        }

        [Fact]
        public void When_GrantType_Is_Not_Defined_Then_Set_To_Authorization_Code()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                }
            };

            // ACT
            _registrationParameterValidator.Validate(parameter);

            // ASSERT
            Assert.NotNull(parameter);
            Assert.True(parameter.GrantTypes.Count == 1);
            Assert.True(parameter.GrantTypes.Contains(Models.GrantType.authorization_code));
        }

        [Fact]
        public void When_Application_Type_Is_Not_Defined_Then_Set_To_Web_Application()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                }
            };

            // ACT
            _registrationParameterValidator.Validate(parameter);

            // ASSERT
            Assert.NotNull(parameter);
            Assert.True(parameter.ApplicationType == ApplicationTypes.web);
        }

        [Fact]
        public void When_Application_Type_Is_Web_And_Redirect_Uri_Is_Not_Https_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "http://google.fr"
                }
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.True(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        [Fact]
        public void When_Application_Type_Is_Web_And_Redirect_Uri_Is_Localhost_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "http://localhost.fr"
                }
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.True(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        [Fact]
        public void When_Application_Type_Is_Native_And_Redirect_Uri_Is_Not_Localhost_Then_Exception_Is_Raised()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "http://google.fr"
                },
                ApplicationType = ApplicationTypes.native
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.True(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        [Fact]
        public void When_Logo_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                LogoUri = "logo_uri"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.LogoUri));
        }

        [Fact]
        public void When_Client_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                ClientUri = "client_uri"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.ClientUri));
        }

        [Fact]
        public void When_Tos_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                TosUri = "tos_uri"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.TosUri));
        }

        [Fact]
        public void When_Jwks_Uri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                JwksUri = "jwks_uri"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.JwksUri));
        }

        [Fact]
        public void When_Set_Jwks_And_Jwks_Uri_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                JwksUri = "http://localhost/identity",
                Jwks = new JsonWebKeySet()
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed);
        }

        [Fact]
        public void When_SectorIdentifierUri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                SectorIdentifierUri = "sector_identifier_uri"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.SectorIdentifierUri));
        }

        [Fact]
        public void When_SectorIdentifierUri_Doesnt_Have_Https_Scheme_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                SectorIdentifierUri = "http://localhost/identity"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.SectorIdentifierUri));
        }

        [Fact]
        public void When_SectorIdentifierUri_Cannot_Be_Retrieved_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                SectorIdentifierUri = "https://localhost/identity"
            };

            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryFake.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);


            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheSectorIdentifierUrisCannotBeRetrieved);
        }

        [Fact]
        public void When_SectorIdentifierUri_Is_Not_A_Redirect_Uri_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                SectorIdentifierUri = "https://localhost/identity"
            };

            var sectorIdentifierUris = new List<string>
            {
                "https://localhost/sector_identifier"
            };
            var json = sectorIdentifierUris.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryFake.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);


            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.OneOrMoreSectorIdentifierUriIsNotARedirectUri);
        }

        [Fact]
        public void When_IdTokenEncryptedResponseEnc_Is_Specified_But_Not_IdTokenEncryptedResponseAlg_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
        }

        [Fact]
        public void When_IdToken_Encrypted_Response_Enc_Is_Specified_And_Id_Token_Encrypted_Response_Alg_Is_Not_Correct_Then_Exception_Is_Thrown ()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                IdTokenEncryptedResponseAlg = "not_correct"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
        }

        [Fact]
        public void When_User_Info_Encrypted_Response_Enc_Is_Specified_And_User_Info_Encrypted_Alg_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                UserInfoEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
        }

        [Fact]
        public void When_User_Info_Encrypted_Response_Enc_Is_Specified_And_User_Info_Encrypted_Alg_Is_Not_Correct_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                UserInfoEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                UserInfoEncryptedResponseAlg = "user_info_encrypted_response_alg_not_correct"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
        }

        [Fact]
        public void When_Request_Object_Encryption_Enc_Is_Specified_And_Request_Object_Encryption_Alg_Is_Not_Set_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                RequestObjectEncryptionEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
        }

        [Fact]
        public void When_Request_Object_Encryption_Enc_Is_Specified_And_Request_Object_Encryption_Alg_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                RequestObjectEncryptionEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                RequestObjectEncryptionAlg = "request_object_encryption_alg_not_valid"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
        }

        [Fact]
        public void When_InitiateLoginUri_Is_Not_Valid_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                InitiateLoginUri = "sector_identifier_uri"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.InitiateLoginUri));
        }

        [Fact]
        public void When_InitiateLoginUri_Doesnt_Have_Https_Scheme_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                InitiateLoginUri = "http://localhost/identity"
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, ClientNames.InitiateLoginUri));
        }

        [Fact]
        public void When_Passing_One_Not_Valid_Request_Uri_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "https://google.fr"
                },
                RequestUris = new List<string>
                {
                    "not_valid_uri"
                }
            };

            // ACT & ASSERTS
            var ex = Assert.Throws<IdentityServerException>(() => _registrationParameterValidator.Validate(parameter));
            Assert.True(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.True(ex.Message == ErrorDescriptions.OneOfTheRequestUriIsNotValid);
        }

        [Fact]
        public void When_Passing_Valid_Request_Then_No_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var parameter = new RegistrationParameter
            {
                RedirectUris = new List<string>
                {
                    "http://localhost"
                },
                ApplicationType = ApplicationTypes.native,
                Jwks = new JsonWebKeySet(),
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.A128KW,
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                UserInfoEncryptedResponseAlg = Jwt.Constants.JweAlgNames.A128KW,
                UserInfoEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                RequestObjectEncryptionAlg = Jwt.Constants.JweAlgNames.A128KW,
                RequestObjectEncryptionEnc = Jwt.Constants.JweEncNames.A128CBC_HS256,
                RequestUris = new List<string>
                {
                    "http://localhost"
                },
                SectorIdentifierUri = "https://localhost"
            };

            var sectorIdentifierUris = new List<string>
            {
                "http://localhost"
            };
            var json = sectorIdentifierUris.SerializeWithJavascript();
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(json)
            };
            var handler = new FakeHttpMessageHandler(httpResponseMessage);
            var httpClientFake = new HttpClient(handler);
            _httpClientFactoryFake.Setup(h => h.GetHttpClient())
                .Returns(httpClientFake);

            // ACT & ASSERTS
            var ex = Record.Exception(() => _registrationParameterValidator.Validate(parameter));
            Assert.Null(ex);
        }

        private void InitializeFakeObjects()
        {
            _httpClientFactoryFake = new Mock<IHttpClientFactory>();
            _registrationParameterValidator = new RegistrationParameterValidator(_httpClientFactoryFake.Object);
        }
    }
}
