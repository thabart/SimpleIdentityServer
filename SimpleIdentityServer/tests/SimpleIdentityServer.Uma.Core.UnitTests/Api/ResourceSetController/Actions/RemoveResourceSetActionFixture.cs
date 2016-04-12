

using Moq;
using SimpleIdentityServer.Uma.Core.Api.ResourceSetController.Actions;
using SimpleIdentityServer.Uma.Core.Errors;
using SimpleIdentityServer.Uma.Core.Exceptions;
using SimpleIdentityServer.Uma.Core.Models;
using SimpleIdentityServer.Uma.Core.Repositories;
using System;
using System.Net;
using Xunit;

namespace SimpleIdentityServer.Uma.Core.UnitTests.Api.ResourceSetController.Actions
{
    public class RemoveResourceSetActionFixture
    {
        private Mock<IResourceSetRepository> _resourceSetRepositoryStub;

        private IDeleteResourceSetAction _deleteResourceSetAction;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACT & ASSERT
            Assert.Throws<ArgumentNullException>(() => _deleteResourceSetAction.Execute(null));
        }

        [Fact]
        public void When_ResourceSet_Doesnt_Exist_Then_False_Is_Returned()
        {
            // ARRANGE
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.GetResourceSetById(It.IsAny<string>()))
                .Returns(() => null);

            // ACT
            var result = _deleteResourceSetAction.Execute(resourceSetId);

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void When_ResourceSet_Cannot_Be_Updated_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.GetResourceSetById(It.IsAny<string>()))
                .Returns(new ResourceSet());
            _resourceSetRepositoryStub.Setup(r => r.DeleteResource(It.IsAny<string>()))
                .Returns(false);

            // ACT & ASSERTS
            var exception = Assert.Throws<BaseUmaException>(() => _deleteResourceSetAction.Execute(resourceSetId));
            Assert.NotNull(exception);
            Assert.True(exception.Code == ErrorCodes.InternalError);
            Assert.True(exception.Message == string.Format(ErrorDescriptions.TheResourceSetCannotBeRemoved, resourceSetId));
        }

        #endregion

        #region Happy path

        [Fact]
        public void When_ResourceSet_Is_Removed_Then_True_Is_Returned()
        {
            // ARRANGE
            const string resourceSetId = "resourceSetId";
            InitializeFakeObjects();
            _resourceSetRepositoryStub.Setup(r => r.GetResourceSetById(It.IsAny<string>()))
                .Returns(new ResourceSet());
            _resourceSetRepositoryStub.Setup(r => r.DeleteResource(It.IsAny<string>()))
               .Returns(true);

            // ACT
            var result = _deleteResourceSetAction.Execute(resourceSetId);

            // ASSERT
            Assert.True(result);
        }

        #endregion

        #region Private methods

        private void InitializeFakeObjects()
        {
            _resourceSetRepositoryStub = new Mock<IResourceSetRepository>();
            _deleteResourceSetAction = new DeleteResourceSetAction(_resourceSetRepositoryStub.Object);
        }

        #endregion
    }
}
