namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IClientSecretBasicAuthentication
    {
        string GetClientId(AuthenticateInstruction instruction);
    }

    public class ClientSecretBasicAuthentication
    {
        public string GetClientId(AuthenticateInstruction instruction)
        {
            return instruction.ClientIdFromAuthorizationHeader;
        }
    }
}
