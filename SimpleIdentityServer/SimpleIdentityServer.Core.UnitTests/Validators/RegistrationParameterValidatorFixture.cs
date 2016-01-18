using System;
using System.Collections.Generic;
using NUnit.Framework;
using SimpleIdentityServer.Core.Errors;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.Jwt.Signature;
using SimpleIdentityServer.Core.Parameters;
using SimpleIdentityServer.Core.Validators;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.UnitTests.Validators
{
    [TestFixture]
    public sealed class RegistrationParameterValidatorFixture
    {
        private IRegistrationParameterValidator _registrationParameterValidator;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _registrationParameterValidator.Validate(null));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.MissingParameter, Constants.StandardRegistrationRequestParameterNames.RequestUris));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheRedirectUriContainsAFragment);
        }

        [Test]
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
            Assert.IsNotNull(parameter);
            Assert.IsTrue(parameter.ResponseTypes.Count == 1);
            Assert.IsTrue(parameter.ResponseTypes.Contains(Core.Models.ResponseType.code));
        }

        [Test]
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
            Assert.IsNotNull(parameter);
            Assert.IsTrue(parameter.GrantTypes.Count == 1);
            Assert.IsTrue(parameter.GrantTypes.Contains(Models.GrantType.authorization_code));
        }

        [Test]
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
            Assert.IsNotNull(parameter);
            Assert.IsTrue(parameter.ApplicationType == ApplicationTypes.web);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidRedirectUri);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheRedirectUriParameterIsNotValid);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.StandardRegistrationRequestParameterNames.LogoUri));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.StandardRegistrationRequestParameterNames.ClientUri));
        }
        
        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.StandardRegistrationRequestParameterNames.TosUri));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.StandardRegistrationRequestParameterNames.JwksUri));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheJwksParameterCannotBeSetBecauseJwksUrlIsUsed);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.StandardRegistrationRequestParameterNames.SectoreIdentifierUri));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.StandardRegistrationRequestParameterNames.SectoreIdentifierUri));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheParameterIsTokenEncryptedResponseAlgMustBeSpecified);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheParameterUserInfoEncryptedResponseAlgMustBeSpecified);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == ErrorDescriptions.TheParameterRequestObjectEncryptionAlgMustBeSpecified);
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.StandardRegistrationRequestParameterNames.InitiateLoginUri));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == string.Format(ErrorDescriptions.ParameterIsNotCorrect, Constants.StandardRegistrationRequestParameterNames.InitiateLoginUri));
        }

        [Test]
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
            Assert.IsTrue(ex.Code == ErrorCodes.InvalidClientMetaData);
            Assert.IsTrue(ex.Message == ErrorDescriptions.OneOfTheRequestUriIsNotValid);
        }

        [Test]
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

            // ACT & ASSERTS
            Assert.DoesNotThrow(() => _registrationParameterValidator.Validate(parameter));
        }

        private void InitializeFakeObjects()
        {
            _registrationParameterValidator = new RegistrationParameterValidator();
        }
    }
}
