﻿using Moq;
using SimpleIdentityServer.Core.Api.Jwks;
using SimpleIdentityServer.Core.Api.Jwks.Actions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Api.Jwks
{
    public sealed class JwksActionsFixture
    {
        private Mock<IGetSetOfPublicKeysUsedToValidateJwsAction> _getSetOfPublicKeysUsedToValidateJwsActionStub;
        private Mock<IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction> _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenActionStub;
        private Mock<IRotateJsonWebKeysOperation> _rotateJsonWebKeysOperationStub;
        private IJwksActions _jwksActions;

        [Fact]
        public async Task When_Retrieving_Jwks_Then_Set_Of_Private_And_Public_Keys_Are_Returned()
        {
            // ARRANGE
            InitializeFakeObjects();
            var publicKeys = new List<Dictionary<string, object>>();
            var privateKeys = new List<Dictionary<string, object>>();

            _getSetOfPublicKeysUsedToValidateJwsActionStub.Setup(g => g.Execute())
                .Returns(Task.FromResult(publicKeys));
            _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenActionStub.Setup(g => g.Execute())
                .Returns(Task.FromResult(privateKeys));

            // ACT
            var result = await _jwksActions.GetJwks().ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public async Task When_JsonWebKeys_Are_Rotated_Then_Operation_Should_Be_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            const bool rotateSuccess = true;
            _rotateJsonWebKeysOperationStub.Setup(r => r.Execute())
                .Returns(Task.FromResult(rotateSuccess));

            // ACT
            var result = await _jwksActions.RotateJwks().ConfigureAwait(false);

            // ASSERT
            Assert.True(result == rotateSuccess);
            _rotateJsonWebKeysOperationStub.Verify(r => r.Execute());
        }

        private void InitializeFakeObjects()
        {
            _getSetOfPublicKeysUsedToValidateJwsActionStub = new Mock<IGetSetOfPublicKeysUsedToValidateJwsAction>();
            _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenActionStub = new Mock<IGetSetOfPublicKeysUsedByTheClientToEncryptJwsTokenAction>();
            _rotateJsonWebKeysOperationStub = new Mock<IRotateJsonWebKeysOperation>();
            _jwksActions = new JwksActions(
                _getSetOfPublicKeysUsedToValidateJwsActionStub.Object,
                _getSetOfPublicKeysUsedByTheClientToEncryptJwsTokenActionStub.Object,
                _rotateJsonWebKeysOperationStub.Object);
        }
    }
}
