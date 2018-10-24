using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Common;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Extensions
{
    public sealed class ClientExtensionsFixture
    {
        #region Test GetIdTokenSignedResponseAlg

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenSignedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenSignedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenSignedResponseAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenSignedResponseAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetIdTokenSignedResponseAlg();

            // ASSERT
            Assert.NotNull(result == JwsAlg.RS256);
        }

        #endregion

        #region Test GetIdTokenEncryptedResponseAlg

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenEncryptedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenEncryptedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenEncryptedResponseAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseAlg();

            // ASSERT
            Assert.True(result == JweAlg.RSA1_5);
        }

        #endregion

        #region Test GetIdTokenEncryptedResponseEnc

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetIdTokenEncryptedResponseEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenEncryptedResponseEnc = "not_supported"
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseEnc();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetIdTokenEncryptedResponseEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                IdTokenEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetIdTokenEncryptedResponseEnc();

            // ASSERT
            Assert.True(result == JweEnc.A128CBC_HS256);
        }

        #endregion

        #region Test GetUserInfoSignedResponseAlg

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoSignedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoSignedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoSignedResponseAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoSignedResponseAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoSignedResponseAlg = Jwt.Constants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetUserInfoSignedResponseAlg();

            // ASSERT
            Assert.True(result == JwsAlg.RS256);
        }

        #endregion

        #region Test GetUserInfoEncryptedResponseAlg

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoEncryptedResponseAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoEncryptedResponseAlg = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoEncryptedResponseAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoEncryptedResponseAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseAlg();

            // ASSERT
            Assert.True(result == JweAlg.RSA1_5);
        }

        #endregion

        #region Test GetUserInfoEncryptedResponseEnc

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetUserInfoEncryptedResponseEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoEncryptedResponseEnc = "not_supported"
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseEnc();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetUserInfoEncryptedResponseEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                UserInfoEncryptedResponseEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetUserInfoEncryptedResponseEnc();

            // ASSERT
            Assert.True(result == JweEnc.A128CBC_HS256);
        }

        #endregion

        #region Test GetRequestObjectSigningAlg

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectSigningAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectSigningAlg = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectSigningAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectSigningAlg_Then_RS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectSigningAlg = Jwt.Constants.JwsAlgNames.RS256
            };

            // ACT
            var result = client.GetRequestObjectSigningAlg();

            // ASSERT
            Assert.True(result == JwsAlg.RS256);
        }

        #endregion

        #region Test GetRequestObjectEncryptionAlg

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectEncryptionAlg_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectEncryptionAlg = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectEncryptionAlg();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectEncryptionAlg_Then_RSA1_5_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectEncryptionAlg = Jwt.Constants.JweAlgNames.RSA1_5
            };

            // ACT
            var result = client.GetRequestObjectEncryptionAlg();

            // ASSERT
            Assert.True(result == JweAlg.RSA1_5);
        }

        #endregion

        #region Test GetRequestObjectEncryptionEnc

        [Fact]
        public void When_Passing_Not_Supported_Alg_To_GetRequestObjectEncryptionEnc_Then_Null_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectEncryptionEnc = "not_supported"
            };

            // ACT
            var result = client.GetRequestObjectEncryptionEnc();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void When_Passing_Alg_To_GetRequestObjectEncryptionEnc_Then_A128CBC_HS256_Is_Returned()
        {
            // ARRANGE
            var client = new Core.Common.Models.Client
            {
                RequestObjectEncryptionEnc = Jwt.Constants.JweEncNames.A128CBC_HS256
            };

            // ACT
            var result = client.GetRequestObjectEncryptionEnc();

            // ASSERT
            Assert.True(result == JweEnc.A128CBC_HS256);
        }

        #endregion
    }
}
