using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Module.Tests.DTOs
{
    [DataContract]
    public class NugetFlatContainerResponse
    {
        [DataMember(Name = "versions")]
        public List<string> Versions { get; set; }
    }
}
