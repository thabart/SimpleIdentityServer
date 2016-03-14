using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.Core.Models;
using Xunit;

namespace SimpleIdentityServer.Core.UnitTests.Extensions
{
    public sealed class ResourceOwnerExtensionsFixture
    {
        [Fact]
        public void When_Passing_ResourceOwner_With_No_Parameter_Set_Then_List_Of_Claims_Is_Returned()
        {
            // ARRANGE
            var resourceOwner = new ResourceOwner();
            var claimNames = new List<string>
            {
                Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified,
                Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified,
                Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt
            };

            // ACT
            var claims = resourceOwner.ToClaims();

            // ASSERT
            Assert.True(claims.All(c => claimNames.Contains(c.Type)));
        }

        [Fact]
        public void When_Passing_Subject_Then_List_Of_Claims_Is_Returned()
        {
            // ARRANGE
            const string subject = "subject";
            const string birthDate = "birthDate";
            const string email = "email";
            const string familyName = "familyName";
            const string gender = "gender";
            const string givenName = "givenName";
            const string locale = "locale";
            const string middleName = "middleName";
            const string name = "name";
            const string nickName = "nickName";
            const string phoneNumber = "phoneNumber";
            const string picture = "picture";
            const string preferredUserName = "preferredUserName";
            const string profile = "profile";
            const string webSite = "webSite";
            const string zoneInfo = "zoneInfo";
            const string roleName = "administrator";
            var address = new Address
            {
                Country = "country"
            };
            const bool emailVerified = false;
            const bool phoneNumberVerified = false;
            const double updatedAt = 200;
            var roles = new List<string>
            {
                roleName
            };
            var serializedAddress = address.SerializeWithDataContract();
            // Set all the other properties !!!
            var resourceOwner = new ResourceOwner
            {
                Id = subject,
                BirthDate = birthDate,
                Email = email,
                FamilyName = familyName,
                Gender = gender,
                GivenName = givenName,
                Locale = locale,
                MiddleName = middleName,
                Name = name,
                NickName = nickName,
                PhoneNumber = phoneNumber,
                Picture = picture,
                PreferredUserName = preferredUserName,
                Profile = profile,
                WebSite = webSite,
                ZoneInfo = zoneInfo,
                Address = address,
                EmailVerified = emailVerified,
                PhoneNumberVerified = phoneNumberVerified,
                UpdatedAt = updatedAt,
                Roles = roles
            };

            var claimNames = new List<string>
            {
                Jwt.Constants.StandardResourceOwnerClaimNames.Subject,
                Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate,
                Jwt.Constants.StandardResourceOwnerClaimNames.Email,
                Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName,
                Jwt.Constants.StandardResourceOwnerClaimNames.Gender,
                Jwt.Constants.StandardResourceOwnerClaimNames.GivenName,
                Jwt.Constants.StandardResourceOwnerClaimNames.Locale,
                Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName,
                Jwt.Constants.StandardResourceOwnerClaimNames.Name,
                Jwt.Constants.StandardResourceOwnerClaimNames.NickName,
                Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber,
                Jwt.Constants.StandardResourceOwnerClaimNames.Picture,
                Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName,
                Jwt.Constants.StandardResourceOwnerClaimNames.Profile,
                Jwt.Constants.StandardResourceOwnerClaimNames.WebSite,
                Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo,
                Jwt.Constants.StandardResourceOwnerClaimNames.Address,
                Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified,
                Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified,
                Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt,
                Jwt.Constants.StandardResourceOwnerClaimNames.Role
            };

            // ACT
            var claims = resourceOwner.ToClaims();

            // ASSERT
            Assert.True(claims.All(c => claimNames.Contains(c.Type)));
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Subject).Value == subject);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate).Value == birthDate);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Email).Value == email);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName).Value == familyName);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Gender).Value == gender);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.GivenName).Value == givenName);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Locale).Value == locale);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName).Value == middleName);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Name).Value == name);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.NickName).Value == nickName);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber).Value == phoneNumber);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Picture).Value == picture);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName).Value == preferredUserName);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Profile).Value == profile);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.WebSite).Value == webSite);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo).Value == zoneInfo);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Address).Value == serializedAddress);
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified).Value == emailVerified.ToString());
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified).Value == phoneNumberVerified.ToString());
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt).Value == updatedAt.ToString(CultureInfo.InvariantCulture));
            Assert.True(claims.First(c => c.Type == Jwt.Constants.StandardResourceOwnerClaimNames.Role).Value == roleName);
        }
    }
}
