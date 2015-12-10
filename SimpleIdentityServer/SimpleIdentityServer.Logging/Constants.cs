using System.Diagnostics.Tracing;

namespace SimpleIdentityServer.Logging
{
    public static class Constants
    {
        public static class EventIds
        {
            public const int AuthorizationStarted = 1;
            public const int AuthorizationEnded = 2;
            public const int OpenIdFailure = 3;
        }

        public static class Tasks
        {
            public const EventTask Authorization = (EventTask) 1;
            public const EventTask Failure = (EventTask) 2;
        }
    }
}
