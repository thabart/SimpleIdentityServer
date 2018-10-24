namespace SimpleIdentityServer.Core.Services
{
    public class DefaultClientPasswordService : IClientPasswordService
    {
        public string Encrypt(string password)
        {
            return password;
        }
    }
}
