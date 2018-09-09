﻿

using Moq;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using SimpleIdentityServer.Uma.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class RemoveResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;
        private Mock<IUmaServerEventSource> _umaServerEventSourceStub;
        private IDeleteResourceSetAction _deleteResourceSetAction;

        [Fact]
        public async Task When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deleteResourceSetAction.Execute(null)).ConfigureAwait(false);
        }

        [Fact]
        public async Task When_ResourceSet_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(() => Task.FromResult((ResourceSet)null));

            // ACT
            var result = await _deleteResourceSetAction.Execute(resourceSetId).ConfigureAwait(false);

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public async Task When_ResourceSet_Cannot_Be_Updated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceSet()));
            _resourceSetRepositoryStub.Setup(r => r.Delete(It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            // ACT & ASSERTS
            var exception = await Assert.ThrowsAsync<BaseUmaException>(() => _deleteResourceSetAction.Execute(resourceSetId)).ConfigureAwait(false);
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetCannotBeRemoved, resourceSetId));
        }

        [Fact]
        public async Task When_ResourceSet_Is_Removed_Then_True_Is_Returned()
        {
            // ARRANGE
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.Get(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceSet()));
            _resourceSetRepositoryStub.Setup(r => r.Delete(It.IsAny<string>()))
               .Returns(Task.FromResult(true));

            // ACT
            var result = await _deleteResourceSetAction.Execute(resourceSetId).ConfigureAwait(false);

            // ASSERT
            Assert.True(result);
        }

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _umaServerEventSourceStub = new Mock<IUmaServerEventSource>();
            _deleteResourceSetAction = new DeleteResourceSetAction(
                _resourceSetRepositoryStub.Object,
                _umaServerEventSourceStub.Object);
        }
    }
}
