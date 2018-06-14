using SimpleIdentityServer.Module;
using System.Collections.Generic;

namespace SimpleIdentityServer.Shell
{
    public class BasicShellOptions
    {
        public IEnumerable<ModuleUIDescriptor> Descriptors { get; set; }
    }
}
