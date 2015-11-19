using System;

namespace SimpleIdentityServer.Core.Exceptions
{
    public class AuthorizationException : IdentityServerException
    {
        public AuthorizationException(string code, string message) : base(code, message)
        {
        }
    }
}
