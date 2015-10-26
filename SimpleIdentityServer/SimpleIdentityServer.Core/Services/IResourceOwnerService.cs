namespace SimpleIdentityServer.Core.Services
{
    public interface IResourceOwnerService
    {
        string Authenticate(string userName, string password);
    }
}
