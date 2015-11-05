namespace SimpleIdentityServer.Core
{
    public static class Constants
    {
        public static class StandardResourceOwnerClaimNames
        {
            public static string Subject = "sub";

            public static string Name = "name";

            public static string GivenName = "given_name";

            public static string FamilyName = "family_name";

            public static string MiddleName = "middle_name";

            public static string NickName = "nickname";

            public static string PreferredUserName = "preferred_username";

            public static string Profile = "profile";

            public static string Picture = "picture";

            public static string WebSite = "website";

            public static string Email = "email";

            public static string EmailVerified = "email_verified";

            public static string Gender = "gender";

            public static string BirthDate = "birthdate";

            public static string ZoneInfo = "zoneinfo";

            public static string Locale = "locale";

            public static string PhoneNumber = "phone_number";

            public static string PhoneNumberVerified = "phone_number_verified";

            public static string Address = "address";

            public static string UpdatedAt = "updated_at";
        }

        // Open-Id Provider Authentication Policy Extension 1.0
        public static class StandardArcParameterNames
        {
            public static string OpenIdNsPage = "openid.ns.pape";

            public static string OpenIdMaxAuthAge = "openid.pape.max_auth_age";

            public static string OpenIdAuthPolicies = "openid.pape.preferred_auth_policies";

            // Namespace for the custom Assurance Level
            public static string OpenIdCustomAuthLevel = "openid.pape.auth_level.ns";
            
            public static string OpenIdPreferredCustomAuthLevel = "openid.pape.preferred_auth_levels";
        }

        // Standard authentication policies.
        // They are coming from the RFC : http://openid.net/specs/openid-provider-authentication-policy-extension-1_0.html
        public static class StandardAuthenticationPolicies
        {
            public static string OpenIdPhishingResistant = "http://schemas.openid.net/pape/policies/2007/06/phishing-resistant";

            // provides more than one authentication factor for example password + software token
            public static string OpenIdMultiFactorAuth = "http://schemas.openid.net/pape/policies/2007/06/multi-factor";

            // provides more than one authentication factor with at least one physical factor
            public static string OpenIdPhysicalMultiFactorAuth = "http://schemas.openid.net/pape/policies/2007/06/multi-factor-physical";
        }

        // Custom authentication policies defined by Simple Identity Server
        public static class CustomAuthenticationPolicies
        {
            public static string CustomPasswordAuth = "http://schemas.simpleidentityserver.net/pape/policies/2015/05/password";
        }

        // Defines the Assurance Level
        // For more information check this documentation : http://csrc.nist.gov/publications/nistpubs/800-63/SP800-63V1_0_2.pdf
        public enum NistAssuranceLevel
        {
            Level1 = 1,
            Level2 = 2,
            Level3 = 3,
            Level4 = 4
        }
    }
}