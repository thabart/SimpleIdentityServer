using SimpleIdentityServer.Manager.Core.Parameters;
using SimpleIdentityServer.Manager.Host.DTOs.Requests;

namespace SimpleIdentityServer.Manager.Host.Extensions
{
    public static class MappingExtensions
    {
        #region To parameters
    
        public static GetJwsParameter ToParameter(GetJwsRequest getJwsRequest)
        {
            return new GetJwsParameter
            {
                Jws = getJwsRequest.Jws,
                Url = getJwsRequest.Url
            };
        }

        #endregion
    }
}
