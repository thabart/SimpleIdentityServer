using Moq;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using SimpleIdentityServer.Core.Jwt;
using SimpleIdentityServer.Core.Repositories;
using System.Collections.Generic;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Jwks.Operations
{
    public sealed class RotateJsonWebKeysOperationFixture
    {
        private Mock<IJsonWebKeyRepository> _jsonWebKeyRepositoryStub;

        private IRotateJsonWebKeysOperation _rotateJsonWebKeysOperation;

        [Fact]
        public void When_There_Is_No_JsonWebKeys_To_Rotate_Then_False_Is_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            _jsonWebKeyRepositoryStub.Setup(j => j.GetAll())
                .Returns(() => null);

            // ACT
            var result = _rotateJsonWebKeysOperation.Execute();

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void When_Rotating_Two_JsonWebKeys_Then_SerializedKeyProperty_Has_Changed()
        {
            // ARRANGE
            InitializeFakeObjects();
            const string firstJsonWebKeySerializedKey = "firstJsonWebKeySerializedKey";
            const string secondJsonWebKeySerializedKey = "secondJsonWebKeySerializedKey";
            var jsonWebKeys = new List<JsonWebKey>
            {
                new JsonWebKey
                {
                    Kid = "1",
                    SerializedKey = firstJsonWebKeySerializedKey
                },
                new JsonWebKey
                {
                    Kid = "2",
                    SerializedKey = secondJsonWebKeySerializedKey
                }
            };
            _jsonWebKeyRepositoryStub.Setup(j => j.GetAll())
                .Returns(() => jsonWebKeys);

            // ACT
            var result = _rotateJsonWebKeysOperation.Execute();

            // ASSERT
            _jsonWebKeyRepositoryStub.Verify(j => j.Update(It.IsAny<JsonWebKey>()));
            Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _jsonWebKeyRepositoryStub = new Mock<IJsonWebKeyRepository>();
            _rotateJsonWebKeysOperation = new RotateJsonWebKeysOperation(_jsonWebKeyRepositoryStub.Object);
        }
    }
}
