namespace SimpleIdentityServer.ResourceManager.API.Host
{
    internal static class Constants
    {
        public const string VolumeId = "rm";
        public const char PathSeparator = '/';
        public static class RouteNames
        {
            public const string ElFinterController = "elfinder";
        }

        public static class MimeNames
        {
            public const string Directory = "directory";
            public const string TextPlain = "text/plain";
        }

        public static class ElFinderDtoNames
        {
            public const string Cmd = "cmd";
            public const string Target = "target";
            public const string Targets = "targets";
            public const string Tree = "tree";
            public const string Init = "init";
            public const string Name = "name";
            public const string Current = "current";
            public const string Source = "src";
            public const string Destination = "dst";
            public const string Cut = "cut";
            public const string Q = "q";
        }

        public static class ElFinderResponseNames
        {
            public const string Api = "api";
            public const string Cwd = "cwd";
            public const string Files = "files";
            public const string NetDrivers = "netDrivers";
            public const string UplMaxSize = "uplMaxSize";
            public const string Options = "options";
            public const string Tree = "tree";
            public const string Added = "added";
            public const string Removed = "removed";
            public const string Select = "select";
            public const string List = "list";
        }

        public static class ElFinderOptionNames
        {
            public const string Path = "path";
            public const string Url = "url";
            public const string Separator = "separator";
            public const string Disabled = "disabled";
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
            public const string ErrOpen = "errOpen";
        }

        public static class Errors
        {
            public const string ErrParamNotValidInt = "the parameter {0} is not a valid integer";
            public const string ErrParamNotSpecified = "the parameter {0} is not specified";
            public const string ErrInsertAsset = "an error occured while trying to insert the asset(s)";
            public const string ErrDuplicateAsset = "an error occured while trying to duplicate the asset(s)";
            public const string ErrUpdateAsset = "an error occured while trying to update the asset";
            public const string ErrPasteAsset = "an error occured while trying to paste the asset(s)";
            public const string ErrCutAsset = "an error occured while trying to cut the asset(s)";
            public const string ErrRemoveAssets = "an error occured while trying to remove the asset(s)";
            public const string ErrTargetsNotFound = "some targets don't exist";
        }

        public static class ElFinderCommands
        {
            public const string Open = "open";
            public const string Parents = "parents";
            public const string Mkdir = "mkdir";
            public const string Rm = "rm";
            public const string Rename = "rename";
            public const string Mkfile = "mkfile";
            public const string Tree = "tree";
            public const string Duplicate = "duplicate";
            public const string Paste = "paste";
            public const string Ls = "ls";
            public const string Search = "search";
        }
    }
}
