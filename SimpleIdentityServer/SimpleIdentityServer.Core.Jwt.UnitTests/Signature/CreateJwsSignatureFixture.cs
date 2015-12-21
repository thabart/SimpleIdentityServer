using NUnit.Framework;
using SimpleIdentityServer.Core.Jwt.Signature;

namespace SimpleIdentityServer.Core.Jwt.UnitTests.Signature
{
    [TestFixture]
    public sealed class CreateJwsSignatureFixture
    {
        private ICreateJwsSignature _createJwsSignature;

        [Test]
        public void When_Algorithm_Is_Not_Supported_Then_Null_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT
            var result = _createJwsSignature.SignWithRsa(JwsAlg.ES512, string.Empty, string.Empty);

            // ASSERT
            Assert.IsNull(result);
        }

        private void InitializeFakeObjects()
        {
            _createJwsSignature = new CreateJwsSignature();   
        }
    }
}
