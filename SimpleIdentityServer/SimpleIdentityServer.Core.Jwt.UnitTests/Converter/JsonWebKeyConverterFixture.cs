using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt.Converter;
using SimpleIdentityServer.Core.Jwt.Exceptions;
using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Core.Jwt.UnitTests.Converter
{
    [TestFixture]
    public sealed class JsonWebKeyConverterFixture
    {
        private IJsonWebKeyConverter _jsonWebKeyConverter;

        [Test]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _jsonWebKeyConverter.ConvertFromJson(null));
        }

        [Test]
        public void When_Passing_Invalid_Json_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ConvertFromJson("invalid"));
            Assert.IsTrue(ex.Message == ErrorDescriptions.JwksCannotBeDeserialied);
        }

        [Test]
        public void When_Passing_JsonWeb_Key_With_Missing_Kid_Then_An_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {
                            Constants.JsonWebKeyParameterNames.KeyTypeName, 
                            Constants.KeyTypeValues.RsaName
                        }
                    }
                }
            };

            var json = jsonWebKeySet.SerializeWithJavascript();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ConvertFromJson(json));
            Assert.IsTrue(ex.Message == ErrorDescriptions.JwkIsInvalid);
        }

        [Test]
        public void When_Passing_JsonWeb_Key_With_Not_Supported_Key_Type_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {
                            Constants.JsonWebKeyParameterNames.KeyTypeName, 
                            "not_supported"
                        },
                        {
                            Constants.JsonWebKeyParameterNames.KeyIdentifierName, 
                            "kid"
                        },
                        {
                            Constants.JsonWebKeyParameterNames.UseName, 
                            Constants.UseValues.Encryption
                        }
                    }
                }
            };

            var json = jsonWebKeySet.SerializeWithJavascript();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ConvertFromJson(json));
            Assert.IsTrue(ex.Message == ErrorDescriptions.JwkIsInvalid);
        }

        [Test]
        public void When_Passing_JsonWeb_Key_With_Not_Supported_Usage_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        {
                            Constants.JsonWebKeyParameterNames.KeyTypeName, 
                            Constants.KeyTypeValues.RsaName
                        },
                        {
                            Constants.JsonWebKeyParameterNames.KeyIdentifierName, 
                            "kid"
                        },
                        {
                            Constants.JsonWebKeyParameterNames.UseName, 
                            "invalid_usage"
                        }
                    }
                }
            };

            var json = jsonWebKeySet.SerializeWithJavascript();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ConvertFromJson(json));
            Assert.IsTrue(ex.Message == ErrorDescriptions.JwkIsInvalid);
        }

        [Test]
        public void When_Passing_JsonWeb_Key_Used_For_The_Signature_With_Rsa_Key_But_Wich_Doesnt_Contain_Modulus_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jsonWebKey = new Dictionary<string, object>
            {
                {
                    Constants.JsonWebKeyParameterNames.KeyTypeName,
                    Constants.KeyTypeValues.RsaName
                },
                {
                    Constants.JsonWebKeyParameterNames.KeyIdentifierName,
                    "kid"
                },
                {
                    Constants.JsonWebKeyParameterNames.UseName,
                    Constants.UseValues.Signature
                }
            };
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    jsonWebKey
                }
            };
            var json = jsonWebKeySet.SerializeWithJavascript();

            // ACT & ASSERTS
            var ex = Assert.Throws<InvalidOperationException>(() => _jsonWebKeyConverter.ConvertFromJson(json));
            Assert.IsTrue(ex.Message == ErrorDescriptions.CannotExtractParametersFromJsonWebKey);
        }

        [Test]
        public void When_Passing_JsonWeb_Key_Used_For_The_Signature_With_Rsa_Key_Then_JsonWeb_Key_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var jsonWebKey = new Dictionary<string, object>
            {
                {
                    Constants.JsonWebKeyParameterNames.KeyTypeName,
                    Constants.KeyTypeValues.RsaName
                },
                {
                    Constants.JsonWebKeyParameterNames.KeyIdentifierName,
                    "kid"
                },
                {
                    Constants.JsonWebKeyParameterNames.UseName,
                    Constants.UseValues.Signature
                }
            };
            var jsonWebKeySet = new JsonWebKeySet
            {
                Keys = new List<Dictionary<string, object>>
                {
                    jsonWebKey
                }
            };
            
            using (var rsa = new RSACryptoServiceProvider())
            {
                var parameters = rsa.ExportParameters(false);
                
                jsonWebKey.Add(Constants.JsonWebKeyParameterNames.RsaKey.ModulusName, parameters.Modulus.Base64EncodeBytes());
                jsonWebKey.Add(Constants.JsonWebKeyParameterNames.RsaKey.ExponentName, parameters.Exponent.Base64EncodeBytes());

                var expectedXml = rsa.ToXmlString(false);

                var json = jsonWebKeySet.SerializeWithJavascript();

                // ACT & ASSERTS
                var result = _jsonWebKeyConverter.ConvertFromJson(json);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Count() == 1);
                Assert.IsTrue(result.First().SerializedKey == expectedXml);
            }
        }

        private void InitializeFakeObjects()
        {
            _jsonWebKeyConverter = new JsonWebKeyConverter();
        }
    }
}
