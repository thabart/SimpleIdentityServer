namespace SimpleIdentityServer.ResourceManager.API.Host
{
    internal static class Constants
    {
        public const string AuthUrl = "http://localhost:60005";
        public const string VolumeId = "rm";
        public const char PathSeparator = '/';

        public static class RouteNames
        {
            public const string ElFinterController = "elfinder";
            public const string EndpointsController = "endpoints";
            public const string ConfigurationController = "configuration";
            public const string ClientsController = "clients";
            public const string ScopesController = "scopes";
            public const string ResourceOwnersController = "resourceowners";
            public const string ResourcesController = "resources";
        }

        public static class SearchClientNames
        {
            public const string ClientIds = "client_ids";
            public const string ClientNames = "client_names";
        }

        public static class SearchNames
        {
            public const string StartIndex = "start_index";
            public const string Count = "count";
            public const string Url = "url";
            public const string Content = "content";
        }

        public static class MimeNames
        {
            public const string Directory = "directory";
            public const string TextPlain = "text/plain";
        }

        public static class ClientNames
        {
            public const string ClientId = "client_id";
            public const string ClientName = "name";
            public const string LogoUri = "logo_uri";
        }

        public static class EndpointNames
        {
            public const string Url = "url";
            public const string Name = "name";
            public const string Description = "description";
            public const string Type = "type";
            public const string CreateDateTime = "create_datetime";
        }

        public static class ElFinderAuthPolRuleNames
        {
            public const string Id = "id";
            public const string OpenIdClients = "openidclients";
            public const string OpenIdClaims = "openidclaims";
            public const string Permissions = "permissions";
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
            public const string Rules = "rules";
            public const string Scopes = "scopes";
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
            public const string Url = "url";
            public const string AuthRules = "authrules";
            public const string OpenIdClients = "openidclients";
            public const string OpenIdClaims = "openidclaims";
            public const string Resource = "resource";
            public const string Permissions = "permissions";
            public const string IdProviders = "idproviders";
        }

        public static class ElFinderIdProviderResponseNames
        {
            public const string Url = "url";
            public const string Name = "name";
            public const string Description = "description";
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
            public const string HasSecurity = "has_security";
            public const string Locked = "locked";
            public const string Tmb = "tmb";
            public const string Alias = "alias";
            public const string Thash = "thash";
            public const string Dim = "dim";
            public const string VolumeId = "volumeid";
        }

        public static class ElFinderResourceNames
        {
            public const string Id = "id";
            public const string IconUri = "icon_uri";
            public const string Name = "name";
            public const string Scopes = "scopes";
            public const string Type = "type";
        }

        public static class ElFinderUmaAuthorizationPolicyRuleNames
        {
            public const string Id = "id";
            public const string ClientIdsAllowed = "clients";
            public const string IsResourceOwnerConsentNeeded = "consent_needed";
            public const string Scopes = "scopes";
            public const string Script = "script";
            public const string Claims = "claims";
        }

        public static class ElFinderClaimNames
        {
            public const string Type = "type";
            public const string Value = "value";
        }

        public static class ElFinderOpenIdClientResponseNames
        {
            public const string ClientId = "client_id";
            public const string LogoUri = "logo_uri";
            public const string ClientName = "client_name";
        }

        public static class ErrorDtoNames
        {
            public const string Error = "error";
            public const string Code = "code";
            public const string Message = "message";
        }

        public static class ElFinderErrors
        {
            public const string Error = "error";
            public const string ErrUnknown = "errUnknown";
            public const string ErrUnknownCmd = "errUnknownCmd";
            public const string ErrTrgFolderNotFound = "errTrgFolderNotFound";
            public const string ErrOpen = "errOpen";
            public const string ErrCreateResource = "errCreateResource";
            public const string ErrNoResource = "errorNoResource";
            public const string ErrUpdateResource = "errorUpdateResource";
        }

        public static class ErrorCodes
        {
            public const string InternalError = "internal_error";
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
            public const string ErrRemoveEndpoint = "an error occured while trying to remove the endpoint(s)";
            public const string ErrNoEndpoint = "no endpoint";
            public const string ErrAuthNotConfigured = "authorization server is not configured";
            public const string ErrManagerApiNotConfigured = "manager API is not configured";
            public const string ErrDeleteClient = "an error occured while trying to remove the client(s)";
            public const string ErrSearchClients = "an error occured while trying to search the client(s)";
            public const string ErrInsertClient = "an error occured while trying to insert the client";
            public const string ErrUpdateClient = "an error occured while trying to update the client";
            public const string ErrDeleteRo = "an error occured while trying to remove the resource owner(s)";
            public const string ErrSearchRos = "an error occured while trying to search the resource owner(s)";
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
            public const string Access = "access";
            public const string Perms = "perms";
            public const string MkPerm = "mkperm";
            public const string OpenIdClients = "openidclients";
            public const string GetResource = "getresource";
            public const string PatchResource = "patchresource";
        }
    }
}
