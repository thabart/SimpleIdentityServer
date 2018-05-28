using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Module.Loader
{
    [DataContract]
    public class PackageConfiguration
    {
        [DataMember(Name = Constants.PackageConfigurationNames.Library)]
        public string Library { get; set; }
        [DataMember(Name = Constants.PackageConfigurationNames.Version)]
        public string Version { get; set; }
    }

    [DataContract]
    public class ModuleConfiguration
    {
        [DataMember(Name = Constants.ModuleConfigurationNames.ModuleName)]
        public string ModuleName { get; set; }
        [DataMember(Name = Constants.ModuleConfigurationNames.Packages)]
        public List<PackageConfiguration> Packages { get; set; }
    }

    [DataContract]
    public class ProjectConfiguration
    {
        [DataMember(Name = Constants.ProjectConfigurationNames.Version)]
        public string Version { get; set; }
        [DataMember(Name = Constants.ProjectConfigurationNames.ProjectName)]
        public string ProjectName { get; set; }
        [DataMember(Name = Constants.ProjectConfigurationNames.Modules)]
        public List<ModuleConfiguration> Modules { get; set; }
    }
}
