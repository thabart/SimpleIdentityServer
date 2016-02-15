using System.Collections.Generic;
using System.Linq;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Parameters;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Extensions
{
    public sealed class ClaimsParameterExtensionsFixture
    {
        [Fact]
        public void When_Trying_To_Retrieve_Standard_Claim_Names_From_EmptyList_Then_Empty_List_Is_Returned()
        {
            // ARRANGE
            var claimsParameter = new ClaimsParameter();

            // ACT
            var claimNames = claimsParameter.GetClaimNames();

            // ASSERT
            Assert.NotNull(claimNames);
            Assert.False(claimNames.Any());
        }

        [Fact]
        public void When_Passing_Standard_Claims_In_UserInfo_And_Trying_To_Retrieve_The_Names_Then_Names_Are_Returned()
        {
            // ARRANGE
            const string notStandardClaimName = "not_standard";
            var claimsParameter = new ClaimsParameter
            {
                UserInfo = new List<ClaimParameter>
                {
                    new ClaimParameter { Name = Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject },
                    new ClaimParameter { Name = notStandardClaimName }
                }
            };

            // ACT
            var claimNames = claimsParameter.GetClaimNames();

            // ASSERT
            Assert.NotNull(claimNames);
            Assert.True(claimNames.Contains(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.False(claimNames.Contains(notStandardClaimName));
        }
    }
}
