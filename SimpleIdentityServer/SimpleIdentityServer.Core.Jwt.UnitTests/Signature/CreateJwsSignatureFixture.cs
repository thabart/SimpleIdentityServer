using NUnit.Framework;

using SimpleIdentityServer.Core.Jwt.Signature;

using System;
using System.Security;

namespace SimpleIdentityServer.Core.Jwt.UnitTests.Signature
{
    [TestFixture]
    public sealed class CreateJwsSignatureFixture
    {
        private ICreateJwsSignature _createJwsSignature;

        #region Rsa algorithm

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
        public void When_Trying_To_Rsa_Sign_With_Invalid_Key_Length_Then_Exception_Is_Thrown()
        {
            // TODO : Implement this unit test.
        }

        [Test]
        public void When_Generate_Correct_Rsa_Signature_And_Checking_It_Then_True_Is_Returned()
        {
            // TODO : implement this UT
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _createJwsSignature = new CreateJwsSignature();   
        }
    }
}
