using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SimpleIdentityServer.Module.Loader.Nuget.DTOs.Responses
{
    [DataContract]
    public class NugetFlatContainerResponse
    {
        [DataMember(Name = Constants.NugetFlatContainerResponseNames.Version)]
        public List<string> Versions { get; set; }
    }
}
