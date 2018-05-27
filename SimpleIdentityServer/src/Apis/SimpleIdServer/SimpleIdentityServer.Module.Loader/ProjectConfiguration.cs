using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Module.Loader
{
    [DataContract]
    public class PackageConfiguration
    {
        [DataMember(Name = "library")]
        public string Library { get; set; }
        [DataMember(Name = "version")]
        public string Version { get; set; }
    }

    [DataContract]
    public class ModuleConfiguration
    {
        [DataMember(Name = "moduleName")]
        public string ModuleName { get; set; }
        [DataMember(Name = "packages")]
        public List<PackageConfiguration> Packages { get; set; }
    }

    [DataContract]
    public class ProjectConfiguration
    {
        [DataMember(Name = "version")]
        public string Version { get; set; }
        [DataMember(Name = "projectName")]
        public string ProjectName { get; set; }
        [DataMember(Name = "modules")]
        public List<ModuleConfiguration> Modules { get; set; }
    }
}
