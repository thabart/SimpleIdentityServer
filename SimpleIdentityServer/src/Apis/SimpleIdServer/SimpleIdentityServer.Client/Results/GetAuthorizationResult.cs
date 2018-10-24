using System;

namespace SimpleIdentityServer.Client.Results
{
    public class GetAuthorizationResult : BaseSidResult
    {
        public Uri Location { get; set; }
    }
}
