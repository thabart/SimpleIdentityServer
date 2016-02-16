using Moq;
using SimpleIdentityServer.Manager.Core.Api.Jws;
using SimpleIdentityServer.Manager.Core.Api.Jws.Actions;
using SimpleIdentityServer.Manager.Core.Parameters;
using System;
using Xunit;

namespace SimpleIdentityServer.Manager.Core.Tests.Api.Jws
{
    public class JwsActionsFixture
    {
        private Mock<IGetJwsInformationAction> _getJwsInformationActionStub;

        private IJwsActions _jwsActions;

        #region Exceptions

        [Fact]
        public void When_Passing_Null_Parameter_To_GetJwsInformation_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter();

            // ACTS & ASSERTS
            Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(null));
            Assert.ThrowsAsync<ArgumentNullException>(() => _jwsActions.GetJwsInformation(getJwsParameter));
        }

        #endregion

        #region Happy paths

        [Fact]
        public void When_Executing_GetJwsInformation_Then_Operation_Is_Called()
        {

            // ARRANGE
            InitializeFakeObjects();
            var getJwsParameter = new GetJwsParameter
            {
                Jws = "jws"
            };

            // ACT
            _jwsActions.GetJwsInformation(getJwsParameter).Wait();

            // ASSERT
            _getJwsInformationActionStub.Verify(g => g.Execute(getJwsParameter));
        }

        #endregion

        private void InitializeFakeObjects()
        {
            _getJwsInformationActionStub = new Mock<IGetJwsInformationAction>();
            _jwsActions = new JwsActions(_getJwsInformationActionStub.Object);
        }
    }
}
