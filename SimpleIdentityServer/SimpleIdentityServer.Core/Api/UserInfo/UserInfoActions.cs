using SimpleIdentityServer.Core.Jwt;

namespace SimpleIdentityServer.Core.Api.UserInfo
{
    public interface IUserInfoActions
    {
        JwsPayload GetUserInformation(string accessToken);
    }

    public class UserInfoActions : IUserInfoActions
    {
        public JwsPayload GetUserInformation(string accessToken)
        {
            return null;
        }
    }
}
