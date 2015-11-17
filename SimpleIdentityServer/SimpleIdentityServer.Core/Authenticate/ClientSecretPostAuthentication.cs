namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IClientSecretPostAuthentication
    {
        string GetClientId(AuthenticateInstruction instruction);
    }

    public class ClientSecretPostAuthentication : IClientSecretPostAuthentication
    {
        public string GetClientId(AuthenticateInstruction instruction)
        {
            return instruction.ClientIdFromHttpRequestBody;
        }
    }
}
