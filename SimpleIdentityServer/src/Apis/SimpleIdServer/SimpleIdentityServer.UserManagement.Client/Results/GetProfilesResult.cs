using SimpleIdentityServer.Common.Client;
using SimpleIdentityServer.UserManagement.Common.Responses;
using System.Collections.Generic;

namespace SimpleIdentityServer.UserManagement.Client.Results
{
    public class GetProfilesResult : BaseResponse
    {
        public IEnumerable<ProfileResponse> Content { get; set; }
    }
}