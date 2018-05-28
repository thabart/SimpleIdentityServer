namespace SimpleIdentityServer.Module.Loader
{
    internal static class Constants
    {
        public static class ProjectConfigurationNames
        {
            public const string Version = "version";
            public const string ProjectName = "projectName";
            public const string Modules = "modules";
        }

        public static class ModuleConfigurationNames
        {
            public const string ModuleName = "moduleName";
            public const string Packages = "packages";
        }

        public static class PackageConfigurationNames
        {
            public const string Library = "library";
            public const string Version = "version";
        }

        public static class NugetConfigurationResponseNames
        {
            public const string Version = "version";
            public const string Resources = "resources";
        }

        public static class NugetConfigurationResourceNames
        {
            public const string Id = "@id";
            public const string Type = "@type";
            public const string Comment = "comment";
        }

        public static class NugetFlatContainerResponseNames
        {
            public const string Version = "version";
        }
    }
}
