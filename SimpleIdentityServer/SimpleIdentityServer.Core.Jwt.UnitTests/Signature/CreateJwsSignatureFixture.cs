using NUnit.Framework;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Jwt.Signature;

using System;
using System.Security;
using System.Security.Cryptography;

namespace SimpleIdentityServer.Core.Jwt.UnitTests.Signature
{
    [TestFixture]
    public sealed class CreateJwsSignatureFixture
    {
        private ICreateJwsSignature _createJwsSignature;

        #region Rsa algorithm

        #region Create the signature

        [Test]
        public void When_Trying_To_Rsa_Sign_With_A_Not_Supported_Algorithm_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _createJwsSignature.SignWithRsa(JwsAlg.ES512, string.Empty, string.Empty);

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_An_Empty_Serialized_Keys_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _createJwsSignature.SignWithRsa(JwsAlg.RS256, string.Empty, string.Empty));
        }

        [Test]
        public void When_Trying_To_Rsa_Sign_With_Not_Xml_Synthax_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var toBeEncrypted = "toBeEncrypted";
            var serializedKeys = "invalid_serialized_keys";

            // ACT & ASSERT
           Assert.Throws<XmlSyntaxException>(() => _createJwsSignature.SignWithRsa(JwsAlg.RS256,
                serializedKeys,
                toBeEncrypted));
        }

        [Test]
        public void When_Generate_Rsa_Signature_Then_String_Is_Returned()
        {
            // ARRANGE
            const string messageToBeSigned = "message_to_be_signed";
            InitializeFakeObjects();
            string serializedKeysXml;
            using (var rsa = new RSACryptoServiceProvider())
            {
                serializedKeysXml = rsa.ToXmlString(true);
            }

            // ACT
            var signedMessage = _createJwsSignature.SignWithRsa(
                JwsAlg.RS256,
                serializedKeysXml,
                messageToBeSigned);

            // ASSERT
            Assert.IsNotNull(signedMessage);
        }

        #endregion

        #region Check the signature

        [Test]
        public void When_Trying_To_Check_The_Signature_With_A_Not_Supported_Algorithm_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _createJwsSignature.VerifyWithRsa(JwsAlg.ES512, string.Empty, string.Empty, new byte[0]);

            // ASSERT
            Assert.IsFalse(result);
        }

        [Test]
        public void When_Passing_Empty_Serialized_Keys_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            
            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _createJwsSignature.VerifyWithRsa(JwsAlg.RS256, 
                string.Empty, 
                string.Empty,
                new byte[0]
                ));
        }

        [Test]
        public void When_Generate_Correct_Rsa_Signature_And_Checking_It_Then_True_Is_Returned()
        {
            // ARRANGE
            const string messageToBeSigned = "message_to_be_signed";
            InitializeFakeObjects();
            string serializedKeysXml;
            using (var rsa = new RSACryptoServiceProvider())
            {
                serializedKeysXml = rsa.ToXmlString(true);
            }

            var signedMessage = _createJwsSignature.SignWithRsa(JwsAlg.RS256,
                serializedKeysXml,
                messageToBeSigned);
            var signature = signedMessage.Base64DecodeBytes();

            // ACT
            var isSignatureCorrect = _createJwsSignature.VerifyWithRsa(JwsAlg.RS256,
                serializedKeysXml,
                messageToBeSigned,
                signature);

            // ASSERT
            Assert.IsTrue(isSignatureCorrect);
        }

        #endregion

        #endregion

        private void InitializeFakeObjects()
        {
            _createJwsSignature = new CreateJwsSignature();   
        }
    }
}
