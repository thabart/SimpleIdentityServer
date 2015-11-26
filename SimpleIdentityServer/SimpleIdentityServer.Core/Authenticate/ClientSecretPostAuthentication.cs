using System;
using SimpleIdentityServer.Core.Models;

namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IClientSecretPostAuthentication
    {
        Client AuthenticateClient(AuthenticateInstruction instruction, Client client);

        string GetClientId(AuthenticateInstruction instruction);
    }

    public class ClientSecretPostAuthentication : IClientSecretPostAuthentication
    {
        public Client AuthenticateClient(AuthenticateInstruction instruction, Client client)
        {
            var sameSecret = string.Compare(client.ClientSecret,
                        instruction.ClientSecretFromHttpRequestBody,
                        StringComparison.InvariantCultureIgnoreCase) == 0;
            return sameSecret ? client : null;
        }
        
        public string GetClientId(AuthenticateInstruction instruction)
        {
            return instruction.ClientIdFromHttpRequestBody;
        }
    }
}
