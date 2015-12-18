using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Parameters;

namespace SimpleIdentityServer.Api.UnitTests.Extensions
{
    [TestFixture]
    public sealed class ClaimsParameterExtensionsFixture
    {
        [Test]
        public void When_Trying_To_Retrieve_Standard_Claim_Names_From_EmptyList_Then_Empty_List_Is_Returned()
        {
            // ARRANGE
            var claimsParameter = new ClaimsParameter();

            // ACT
            var claimNames = claimsParameter.GetClaimNames();

            // ASSERT
            Assert.IsNotNull(claimNames);
            Assert.IsFalse(claimNames.Any());
        }

        [Test]
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
            Assert.IsNotNull(claimNames);
            Assert.IsTrue(claimNames.Contains(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject));
            Assert.IsFalse(claimNames.Contains(notStandardClaimName));
        }
    }
}
