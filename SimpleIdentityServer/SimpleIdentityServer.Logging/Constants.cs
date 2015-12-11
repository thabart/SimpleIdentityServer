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
            public const int AuthorizationCodeFlowEnded = 5;
            public const int AuthorizationEnded = 6;
            public const int OpenIdFailure = 7;
        }

        public static class Tasks
        {
            public const EventTask Authorization = (EventTask) 1;
            public const EventTask Failure = (EventTask) 2;
        }
    }
}
