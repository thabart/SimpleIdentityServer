using System.Collections.Generic;

namespace SimpleIdentityServer.Module
{
    public interface IModule
    {
        void Init(IDictionary<string, string> options);
    }
}