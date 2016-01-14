using NUnit.Framework;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.UnitTests.Extensions
{
    [TestFixture]
    public sealed class ClientExtensionsFixture
    {
        #region Test GetIdTokenSignedResponseAlg

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenSignedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                IdTokenSignedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenSignedResponseAlg();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetIdTokenSignedResponseAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetIdTokenSignedResponseAlg();

            // ASSERT
            Assert.IsNotNull(result == JwsAlg.RS256);
        }

        #endregion

        #region Test GetIdTokenEncryptedResponseAlg

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenEncryptedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                IdTokenEncryptedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseAlg();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetIdTokenEncryptedResponseAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseAlg();

            // ASSERT
            Assert.IsTrue(result == JweAlg.RSA1_5);
        }

        #endregion

        #region Test GetIdTokenEncryptedResponseEnc

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenEncryptedResponseEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                IdTokenEncryptedResponseEnc = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseEnc();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetIdTokenEncryptedResponseEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseEnc();

            // ASSERT
            Assert.IsTrue(result == JweEnc.A128CBC_HS256);
        }

        #endregion

        #region Test GetUserInfoSignedResponseAlg

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoSignedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                UserInfoSignedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoSignedResponseAlg();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetUserInfoSignedResponseAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                UserInfoSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetUserInfoSignedResponseAlg();

            // ASSERT
            Assert.IsTrue(result == JwsAlg.RS256);
        }

        #endregion

        #region Test GetUserInfoEncryptedResponseAlg

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoEncryptedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                UserInfoEncryptedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseAlg();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetUserInfoEncryptedResponseAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                UserInfoEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseAlg();

            // ASSERT
            Assert.IsTrue(result == JweAlg.RSA1_5);
        }

        #endregion

        #region Test GetUserInfoEncryptedResponseEnc

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoEncryptedResponseEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                UserInfoEncryptedResponseEnc = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseEnc();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetUserInfoEncryptedResponseEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                UserInfoEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseEnc();

            // ASSERT
            Assert.IsTrue(result == JweEnc.A128CBC_HS256);
        }

        #endregion

        #region Test GetRequestObjectSigningAlg

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectSigningAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                RequestObjectSigningAlg = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectSigningAlg();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetRequestObjectSigningAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                RequestObjectSigningAlg = Jwt.Constants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetRequestObjectSigningAlg();

            // ASSERT
            Assert.IsTrue(result == JwsAlg.RS256);
        }

        #endregion

        #region Test GetRequestObjectEncryptionAlg

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectEncryptionAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                RequestObjectEncryptionAlg = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectEncryptionAlg();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetRequestObjectEncryptionAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                RequestObjectEncryptionAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetRequestObjectEncryptionAlg();

            // ASSERT
            Assert.IsTrue(result == JweAlg.RSA1_5);
        }

        #endregion

        #region Test GetRequestObjectEncryptionEnc

        [Test]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectEncryptionEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                RequestObjectEncryptionEnc = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectEncryptionEnc();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_Alg_To_GetRequestObjectEncryptionEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Client
            {
                RequestObjectEncryptionEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetRequestObjectEncryptionEnc();

            // ASSERT
            Assert.IsTrue(result == JweEnc.A128CBC_HS256);
        }

        #endregion
    }
}
