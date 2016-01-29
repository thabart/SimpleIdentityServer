using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;

using System.Threading;
using System.Threading.Tasks;

using System.Web.Http.Filters;
using SimpleIdentityServer.Core.Common.Extensions;
using SimpleIdentityServer.Core.Extensions;
using SimpleIdentityServer.DataAccess.Fake.Models;

namespace SimpleIdentityServer.Api.Tests.Common.Fakes
{
    public class FakeAuthenticationFilter : IAuthenticationFilter
    {
        public ResourceOwner ResourceOwner;

        public bool AllowMultiple { get; private set; }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            if (ResourceOwner == null)
            {
                return Task.FromResult(0);
            }

            var claims = new List<Claim>();

            // Add the standard open-id claims.
            claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Subject, ResourceOwner.Id));
            if (!string.IsNullOrWhiteSpace(ResourceOwner.BirthDate))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.BirthDate, ResourceOwner.BirthDate));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.Email))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Email, ResourceOwner.Email));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.FamilyName))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.FamilyName, ResourceOwner.FamilyName));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.Gender))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Gender, ResourceOwner.Gender));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.GivenName))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.GivenName, ResourceOwner.GivenName));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.Locale))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Locale, ResourceOwner.Locale));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.MiddleName))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.MiddleName, ResourceOwner.MiddleName));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.Name))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Name, ResourceOwner.Name));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.NickName))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.NickName, ResourceOwner.NickName));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.PhoneNumber))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumber, ResourceOwner.PhoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.Picture))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Picture, ResourceOwner.Picture));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.PreferredUserName))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PreferredUserName, ResourceOwner.PreferredUserName));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.Profile))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Profile, ResourceOwner.Profile));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.WebSite))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.WebSite, ResourceOwner.WebSite));
            }

            if (!string.IsNullOrWhiteSpace(ResourceOwner.ZoneInfo))
            {
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.ZoneInfo, ResourceOwner.ZoneInfo));
            }

            var address = ResourceOwner.Address;
            if (address != null)
            {
                var serializedAddress = address.SerializeWithDataContract();
                claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.Address, serializedAddress));
            }

            claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.EmailVerified, ResourceOwner.EmailVerified.ToString(CultureInfo.InvariantCulture)));
            claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.PhoneNumberVerified, ResourceOwner.PhoneNumberVerified.ToString(CultureInfo.InvariantCulture)));
            claims.Add(new Claim(Core.Jwt.Constants.StandardResourceOwnerClaimNames.UpdatedAt, ResourceOwner.UpdatedAt.ToString(CultureInfo.InvariantCulture)));
            claims.Add(new Claim(ClaimTypes.AuthenticationInstant, DateTime.Now.ConvertToUnixTimestamp().ToString(CultureInfo.InvariantCulture)));

            var identity = new ClaimsIdentity(claims, "FakeApi");
            context.Principal = new ClaimsPrincipal(identity);
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
