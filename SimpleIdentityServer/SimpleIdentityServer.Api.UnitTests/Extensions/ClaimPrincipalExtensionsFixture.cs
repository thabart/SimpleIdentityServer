using System.Collections.Generic;
using System.Security.Claims;

using NUnit.Framework;

using SimpleIdentityServer.Core.Extensions;

namespace SimpleIdentityServer.Api.UnitTests.Extensions
{
    [TestFixture]
    public sealed class ClaimPrincipalExtensionsFixture
    {
        [Test]
        public void When_Passing_Entity_With_No_Identity_And_Called_GetSubject_Then_Null_Is_Returned()
        {
            // ARRANGE
            var claimsPrincipal = new ClaimsPrincipal();
            
            // ACT
            var result = claimsPrincipal.GetSubject();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_ClaimsPrincipal_With_No_Subject_And_Calling_GetSubject_Then_Null_Is_Returned()
        {
            // ARRANGE
            var claims = new List<Claim>();
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            var result = claimsPrincipal.GetSubject();

            // ASSERT
            Assert.IsNull(result);
        }

        [Test]
        public void When_Passing_ClaimsPrincipal_With_NameIdentifier_And_Calling_GetSubject_Then_NameIdentitifer_Is_Returned()
        {
            const string subject = "subject";
            // ARRANGE
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            var result = claimsPrincipal.GetSubject();

            // ASSERT
            Assert.IsTrue(result == subject);
        }

        [Test]
        public void When_Passing_ClaimsPrincipal_With_Subject_And_Calling_GetSubject_Then_Subject_Is_Returned()
        {
            const string subject = "subject";
            // ARRANGE
            var claims = new List<Claim>
            {
                new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, subject)
            };
            var claimsIdentity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            var result = claimsPrincipal.GetSubject();

            // ASSERT
            Assert.IsTrue(result == subject);
        }

        [Test]
        public void When_Passing_No_Claims_Principal_And_Calling_IsAuthenticated_Then_False_Is_Returned()
        {
            // ARRANGE
            var claimsPrincipal = new ClaimsPrincipal();

            // ACT
            var result = claimsPrincipal.IsAuthenticated();

            // ASSERT
            Assert.IsFalse(result);
        }

        [Test]
        public void When_Passing_Claims_Principal_And_Calling_IsAuthenticated_Then_True_Is_Returned()
        {
            // ARRANGE
            var claimsIdentity = new ClaimsIdentity("simpleIdentityServer");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // ACT
            var result = claimsPrincipal.IsAuthenticated();

            // ASSERT
            Assert.IsTrue(result);
        }
    }
}
