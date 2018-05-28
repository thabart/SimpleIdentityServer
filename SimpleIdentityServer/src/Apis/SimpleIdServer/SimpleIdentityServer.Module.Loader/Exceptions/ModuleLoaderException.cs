using System;

namespace SimpleIdentityServer.Module.Loader.Exceptions
{
    public class ModuleLoaderException : Exception
    {
        public ModuleLoaderException(string message) : base(message) { }
    }
}
