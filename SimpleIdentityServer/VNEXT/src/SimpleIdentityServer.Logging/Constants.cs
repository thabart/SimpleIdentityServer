using System.Diagnostics.Tracing;

namespace SimpleIdentityServer.Logging
{
    public static class Constants
    {
        public static class EventIds
        {
            public const int AuthorizationStarted = 1;
            public const int AuthorizationCodeFlowStarted = 2;
            public const int StartProcessingAuthorizationRequest = 3;
            public const int EndProcessingAuthorizationRequest = 4;
            public const int StartGeneratingAuthorizationResponseToClient = 5;
            public const int GrantAccessToClient = 6;
            public const int GrantAuthorizationCodeToClient = 7;
            public const int EndGeneratingAuthorizationResponseToClient = 8;
            public const int AuthorizationCodeFlowEnded = 9;
            public const int ImplicitFlowStart = 10;
            public const int ImplicitFlowEnd = 11;
            public const int HybridFlowStart = 12;
            public const int HybridFlowEnd = 13;
            public const int AuthorizationEnded = 14;
            public const int StartResourceOwnerCredentialsGrantType = 15;
            public const int EndResourceOwnerCredentialsGrantType = 16;
            public const int StartAuthorizationCodeGrantType = 17;
            public const int EndAuthorizationCodeGrantType = 18;
            public const int StartToAuthenticateTheClient = 19;
            public const int FinishToAuthenticateTheClient = 20;
            public const int ResourceOwnerIsAuthenticated = 21;
            public const int ConsentHasBeenGivenByResourceOwner = 22;
            public const int OpenIdFailure = 23;
            public const int Failure = 24;
            public const int StartRegistration = 25;
            public const int EndRegistration = 26;
            public const int StartRefreshTokenGrantType = 27;
            public const int EndRefreshTokenGrantType = 28;
        }

        public static class Tasks
        {
            public const EventTask Authorization = (EventTask) 1;
            public const EventTask Token = (EventTask)2;
            public const EventTask Failure = (EventTask) 3;
            public const EventTask Grant = (EventTask) 4;
            public const EventTask Authenticate = (EventTask) 5;
            public const EventTask Consent = (EventTask)6;
            public const EventTask Registration = (EventTask)7;
        }
    }
}
