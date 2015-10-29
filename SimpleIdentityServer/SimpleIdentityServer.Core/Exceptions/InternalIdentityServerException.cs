using System;

namespace SimpleIdentityServer.Core.Exceptions
{
    public class InternalIdentityServerException : Exception
    {        
        public InternalIdentityServerException(string message) : base(message)
        {
        }

        public InternalIdentityServerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
