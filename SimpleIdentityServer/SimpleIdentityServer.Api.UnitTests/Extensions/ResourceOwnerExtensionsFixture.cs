using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Api.UnitTests.Extensions
{
    [TestFixture]
    public sealed class ResourceOwnerExtensionsFixture
    {
        [Test]
        public void When_Passing_ResourceOwner_With_No_Parameter_Set_Then_List_Of_Claims_Is_Returned()
        {
            // ARRANGE
            var resourceOwner = new ResourceOwner();
            var claimNames = new List<string>
            {
                Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified,
                Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified,
                Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt
            };

            // ACT
            var claims = resourceOwner.ToClaims();

            // ASSERT
            Assert.IsTrue(claims.All(c => claimNames.Contains(c.Type)));
        }

        [Test]
        public void When_Passing_Subject_Then_List_Of_Claims_Is_Returned()
        {
            // ARRANGE
            const string subject = "subject";
            var resourceOwner = new ResourceOwner
            {
                Id = subject
            };
            var claimNames = new List<string>
            {
                Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified,
                Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified,
                Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt,
                Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject
            };

            // ACT
            var claims = resourceOwner.ToClaims();

            // ASSERT
            Assert.IsTrue(claims.All(c => claimNames.Contains(c.Type)));
            Assert.IsTrue(claims.First(c => c.Type == Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value == subject);
        }
    }
}
