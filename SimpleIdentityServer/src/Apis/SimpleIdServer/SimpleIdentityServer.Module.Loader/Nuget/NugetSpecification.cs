using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SimpleIdentityServer.Module.Loader.Nuget
{
    [Serializable]
    public class NugetDependency
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
        [XmlAttribute("version")]
        public string Version { get; set; }
    }

    [Serializable]
    public class NugetGroup
    {
        [XmlAttribute("targetFramework")]
        public string TargetFramework { get; set; }
        [XmlElement("dependency")]
        public List<NugetDependency> Dependencies { get; set; }
    }

    [Serializable]
    public class NugetDependencies
    {
        [XmlElement(ElementName = "group")]
        public List<NugetGroup> Groups { get; set; }
    }

    [Serializable]
    public class NugetMetadata
    {
        [XmlElement("id")]
        public string Id { get; set; }
        [XmlElement("version")]
        public string Version { get; set; }
        [XmlElement("dependencies")]
        public NugetDependencies Dependencies { get; set; }
    }

    [Serializable]
    [XmlRoot("package", Namespace = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd")]
    public class NugetSpecification2013
    {
        [XmlElement("metadata")]
        public NugetMetadata Metadata { get; set; }
    }

    [Serializable]
    [XmlRoot("package", Namespace = "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd")]
    public class NugetSpecification2012
    {
        [XmlElement("metadata")]
        public NugetMetadata Metadata { get; set; }
    }
}
