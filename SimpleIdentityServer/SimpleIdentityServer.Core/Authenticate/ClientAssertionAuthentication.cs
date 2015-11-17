namespace SimpleIdentityServer.Core.Authenticate
{
    public interface IClientAssertionAuthentication
    {
        string GetClientId(AuthenticateInstruction instruction);
    }

    public class ClientAssertionAuthentication : IClientAssertionAuthentication
    {
        public string GetClientId(AuthenticateInstruction instruction)
        {
            if (instruction.ClientAssertionType != Constants.StandardClientAssertionTypes.JwtBearer)
            {
                return null;
            }

            var assertion = instruction.ClientAssertion;

            return null;
        }
    }
}
