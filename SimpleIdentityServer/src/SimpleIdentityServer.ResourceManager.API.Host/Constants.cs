namespace SimpleIdentityServer.ResourceManager.API.Host
{
    internal static class Constants
    {
        public static class RouteNames
        {
            public const string ElFinterController = "elfinder";
        }

        public static class ElFinderDtoNames
        {
            public const string Cmd = "cmd";
            public const string Target = "target";
            public const string Tree = "tree";
            public const string Init = "init";
        }

        public static class ElFinderResponseNames
        {
            public const string Api = "api";
            public const string Cwd = "cwd";
            public const string Files = "files";
            public const string NetDrivers = "netDrivers";
            public const string UplMaxSize = "uplMaxSize";
            public const string Options = "options";
        }

        public static class ElFinderCwdResponseNames
        {
            public const string Name = "name";
            public const string Hash = "hash";
            public const string Phash = "phash";
            public const string Mime = "mime";
            public const string Ts = "ts";
            public const string Date = "date";
            public const string Size = "size";
            public const string Dirs = "dirs";
            public const string Read = "read";
            public const string Write = "write";
            public const string Locked = "locked";
            public const string Tmb = "tmb";
            public const string Alias = "alias";
            public const string Thash = "thash";
            public const string Dim = "dim";
            public const string VolumeId = "volumeid";
        }

        public static class ErrorDtoNames
        {
            public const string Error = "error";
        }

        public static class ElFinderErrors
        {
            public const string Error = "error";
            public const string ErrUnknown = "errUnknown";
            public const string ErrUnknownCmd = "errUnknownCmd";
            public const string ErrTrgFolderNotFound = "errTrgFolderNotFound";
        }

        public static class Errors
        {
            public const string ErrParamNotValidInt = "the parameter {0} is not a valid integer";
            public const string ErrParamNotSpecified = "the parameter {0} is not specified";
        }

        public static class ElFinderCommands
        {
            public const string Open = "open";
        }
    }
}
