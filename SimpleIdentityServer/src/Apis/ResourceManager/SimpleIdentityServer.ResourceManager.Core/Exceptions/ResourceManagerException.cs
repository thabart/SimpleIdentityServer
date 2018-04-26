using System;

namespace SimpleIdentityServer.ResourceManager.Core.Exceptions
{
    public class ResourceManagerException : Exception
    {
        public ResourceManagerException(string code, string message) : base(message)
        {
            Code = code;
        }

        public ResourceManagerException(string code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        public string Code { get; private set; }
    }
}
