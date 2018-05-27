using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Module.Tests.DTOs
{
    [DataContract]
    public class NugetConfigurationResource
    {
        [DataMember(Name = "@id")]
        public string Id { get; set; }
        [DataMember(Name = "@type")]
        public string Type { get; set; }
        [DataMember(Name = "comment")]
        public string Comment { get; set; }
    }

    [DataContract]
    public class NugetConfigurationResponse
    {
        [DataMember(Name = "version")]
        public string Version { get; set; }
        [DataMember(Name = "resources")]
        public List<NugetConfigurationResource> Resources { get; set; }
    }
}
