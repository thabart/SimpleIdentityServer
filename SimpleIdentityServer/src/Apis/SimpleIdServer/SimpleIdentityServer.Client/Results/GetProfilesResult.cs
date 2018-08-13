using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.Core.Common.DTOs.Responses;
using System.Collections.Generic;

namespace SimpleIdentityServer.Client.Results
{
    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}