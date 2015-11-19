using SimpleIdentityServer.Core.Api.UserInfo.Actions;
using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Core.Api.UserInfo
{
    public interface IUserInfoActions
    {
        JwsPayload GetUserInformation(string accessToken);
    }

    public class UserInfoActions : IUserInfoActions
    {
        private readonly IGetJwsPayload _getJwsPayload;

        public UserInfoActions(IGetJwsPayload getJwsPayload)
        {
            _getJwsPayload = getJwsPayload;
        }

        public JwsPayload GetUserInformation(string accessToken)
        {
            return _getJwsPayload.Execute(accessToken);
        }
    }
}
