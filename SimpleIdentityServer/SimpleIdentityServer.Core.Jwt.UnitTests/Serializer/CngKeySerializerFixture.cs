using NUnit.Framework;

using SimpleIdentityServer.Core.Jwt.Serializer;

using System;

using System.Security.Cryptography;

namespace SimpleIdentityServer.Core.Jwt.UnitTests.Serializer
{
    [TestFixture]
    public sealed class CngKeySerializerFixture
    {
        private ICngKeySerializer _cngKeySerializer;

        #region Serialize with private key

        [Test]
        public void When_Passing_Null_To_Serialize_With_Private_Key_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _cngKeySerializer.SerializeCngKeyWithPrivateKey(null));
        }

        [Test]
        public void When_Private_Key_Cannot_Be_Extracted_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var keyCreationParameter = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.None
            };

            var cnk = CngKey.Create(CngAlgorithm.ECDsaP256, null, keyCreationParameter);

            // ACT & ASSERT
            Assert.Throws<CryptographicException>(() => _cngKeySerializer.SerializeCngKeyWithPrivateKey(cnk));
        }

        [Test]
        public void When_Serialize_The_CngKey_With_Private_Key_Then_String_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var keyCreationParameter = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextExport
            };

            var cnk = CngKey.Create(CngAlgorithm.ECDsaP256, null, keyCreationParameter);

            // ACT
            var result = _cngKeySerializer.SerializeCngKeyWithPrivateKey(cnk);

            // ASSERT
            Assert.IsNotNull(result);
        }

        #endregion

        #region Deserialize with private key

        [Test]
        public void When_Passing_Null_To_Deserialize_Function_With_Private_Key_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _cngKeySerializer.DeserializeCngKeyWithPrivateKey(null));
        }

        [Test]
        public void When_Deserialize_Xml_With_Private_Key_Then_CngKey_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            var keyCreationParameter = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextExport
            };

            var cnk = CngKey.Create(CngAlgorithm.ECDsaP256, null, keyCreationParameter);
            var serializedXml = _cngKeySerializer.SerializeCngKeyWithPrivateKey(cnk);

            // ACT
            var result = _cngKeySerializer.DeserializeCngKeyWithPrivateKey(serializedXml);

            // ASSERT
            Assert.IsNotNull(result);
        }

        #endregion

        #region Serialize with public key

        [Test]
        public void When_Passing_Null_To_Serialize_With_Public_Key_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _cngKeySerializer.SerializeCngKeyWithPublicKey(null));
        }
        
        [Test]
        public void When_Serialize_The_CngKey_With_Public_Key_Then_String_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var keyCreationParameter = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextExport
            };

            var cnk = CngKey.Create(CngAlgorithm.ECDsaP256, null, keyCreationParameter);

            // ACT
            var result = _cngKeySerializer.SerializeCngKeyWithPublicKey(cnk);

            // ASSERT
            Assert.IsNotNull(result);
        }

        #endregion

        #region Deserialize with public key

        [Test]
        public void When_Passing_Null_Parameter_To_Deserialize_With_Public_Key_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _cngKeySerializer.DeserializeCngKeyWithPublicKey(null));
        }

        [Test]
        public void When_Deserialize_With_Public_Key_Then_String_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            
            var keyCreationParameter = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.None
            };

            var cnk = CngKey.Create(CngAlgorithm.ECDsaP521, null, keyCreationParameter);

            var serialized = _cngKeySerializer.SerializeCngKeyWithPublicKey(cnk);

            // ACT
            var result = _cngKeySerializer.DeserializeCngKeyWithPublicKey(serialized);

            // ASSERT
            Assert.IsNotNull(result);
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _cngKeySerializer = new CngKeySerializer();
        }
    }
}
