namespace SimpleIdentityServer.ResourceManager.Core.Exceptions
{
    public class ResourceManagerInternalException : ResourceManagerException
    {
        public ResourceManagerInternalException(string code, string message) : base(code, message)
        {
        }
    }
}
