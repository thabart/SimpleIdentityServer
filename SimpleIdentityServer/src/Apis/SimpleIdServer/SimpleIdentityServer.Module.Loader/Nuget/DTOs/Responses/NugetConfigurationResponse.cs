using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Module.Loader.Nuget.DTOs.Responses
{
    [DataContract]
    public class NugetConfigurationResource
    {
        [DataMember(Name = Constants.NugetConfigurationResourceNames.Id)]
        public string Id { get; set; }
        [DataMember(Name = Constants.NugetConfigurationResourceNames.Type)]
        public string Type { get; set; }
        [DataMember(Name = Constants.NugetConfigurationResourceNames.Comment)]
        public string Comment { get; set; }
    }

    [DataContract]
    public class NugetConfigurationResponse
    {
        [DataMember(Name = Constants.NugetConfigurationResponseNames.Version)]
        public string Version { get; set; }
        [DataMember(Name = Constants.NugetConfigurationResponseNames.Resources)]
        public List<NugetConfigurationResource> Resources { get; set; }
    }
}
