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

        [Test]
        public void When_Passing_Null_To_Serialize_Function_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _cngKeySerializer.Serialize(null));
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
            Assert.Throws<CryptographicException>(() => _cngKeySerializer.Serialize(cnk));
        }

        [Test]
        public void When_Serialize_The_CngKey_Then_String_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var keyCreationParameter = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextExport
            };

            var cnk = CngKey.Create(CngAlgorithm.ECDsaP256, null, keyCreationParameter);

            // ACT
            var result = _cngKeySerializer.Serialize(cnk);

            // ASSERT
            Assert.IsNotNull(result);
        }

        [Test]
        public void When_Passing_Null_To_Deserialize_Function_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _cngKeySerializer.Deserialize(null));
        }

        [Test]
        public void When_Deserialize_Xml_Then_CngKey_Is_Returned()
        {            
            // ARRANGE
            InitializeFakeObjects();
            var keyCreationParameter = new CngKeyCreationParameters
            {
                ExportPolicy = CngExportPolicies.AllowPlaintextExport
            };

            var cnk = CngKey.Create(CngAlgorithm.ECDsaP256, null, keyCreationParameter);
            var serializedXml = _cngKeySerializer.Serialize(cnk);

            // ACT
            var result = _cngKeySerializer.Deserialize(serializedXml);

            // ASSERT
            Assert.IsNotNull(result);
        }

        private void InitializeFakeObjects()
        {
            _cngKeySerializer = new CngKeySerializer();
        }
    }
}
