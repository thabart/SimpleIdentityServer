using System;

namespace SimpleIdentityServer.Core.Exceptions
{
    public class IdentityServerException : Exception
    {
        public IdentityServerException(string code, string message) : base(message)
        {
            Code = code;
        }

        public IdentityServerException(string code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        public string Code { get; private set; }
    }
}
