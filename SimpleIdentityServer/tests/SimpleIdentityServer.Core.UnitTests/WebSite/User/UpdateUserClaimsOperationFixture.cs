using Moq;
using SimpleIdentityServer.Core.Common.Models;
using SimpleIdentityServer.Core.Common.Repositories;
using SimpleIdentityServer.Core.Exceptions;
using SimpleIdentityServer.Core.WebSite.User.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.WebSite.User
{
    public class UpdateUserClaimsOperationFixture
    {
        private Mock<IResourceOwnerRepository> _resourceOwnerRepositoryStub;
        private Mock<IClaimRepository> _claimRepositoryStub;
        private IUpdateUserClaimsOperation _updateUserClaimsOperation;

        [Fact]
        public async Task When_Pass_Null_Parameters_Then_Exceptions_Are_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();

            // ACTS & ASSERTS
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserClaimsOperation.Execute(null, null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _updateUserClaimsOperation.Execute("subject", null));
        }

        [Fact]
        public async Task When_ResourceOwner_DoesntExist_Then_Exception_Is_Thrown()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult((ResourceOwner)null));

            // ACT
            var exception = await Assert.ThrowsAsync<IdentityServerException>(() => _updateUserClaimsOperation.Execute("subject", new List<ClaimAggregate>()));

            // ASSERTS
            Assert.NotNull(exception);
            Assert.True(exception.Code == Errors.ErrorCodes.InternalError);
            Assert.True(exception.Message == Errors.ErrorDescriptions.TheRoDoesntExist);
        }

        [Fact]
        public async Task When_Claims_Are_Updated_Then_Operation_Is_Called()
        {
            // ARRANGE
            InitializeFakeObjects();
            _resourceOwnerRepositoryStub.Setup(r => r.GetAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new ResourceOwner
                {
                    Claims = new List<Claim>
                    {
                        new Claim("type", "value"),
                        new Claim("type1", "value")
                    }
                }));
            _claimRepositoryStub.Setup(r => r.GetAllAsync()).Returns(Task.FromResult((IEnumerable<ClaimAggregate>)new List<ClaimAggregate>
            {
                new ClaimAggregate
                {
                    Code = "type"
                }
            }));

            // ACT
            await _updateUserClaimsOperation.Execute("subjet", new List<ClaimAggregate>
            {
                new ClaimAggregate("type", "value1")
            });

            // ASSERT
            _resourceOwnerRepositoryStub.Verify(p => p.UpdateAsync(It.Is<ResourceOwner>(r => r.Claims.Any(c => c.Type == "type" && c.Value == "value1"))));
        }

        private void InitializeFakeObjects()
        {
            _resourceOwnerRepositoryStub = new Mock<IResourceOwnerRepository>();
            _claimRepositoryStub = new Mock<IClaimRepository>();
            _updateUserClaimsOperation = new UpdateUserClaimsOperation(_resourceOwnerRepositoryStub.Object,
                _claimRepositoryStub.Object);
        }
    }
}
