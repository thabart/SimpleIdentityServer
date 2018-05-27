using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Module.Loader
{
    [DataContract]
    public class LibraryResolvedConfiguration
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "references")]
        public List<string> References { get; set; }
    }

    [DataContract]
    public class ProjectResolvedConfiguration
    {
        [DataMember(Name = "libraries")]
        public List<LibraryResolvedConfiguration> Libraries { get; set; }
    }
}
