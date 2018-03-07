using System.Collections.Generic;

namespace SimpleIdentityServer.ResourceManager.Core.Parameters
{
    public enum AssetLevelTypes
    {
        ALL,
        ROOT
    }

    public class SearchAssetsParameter
    {
        public SearchAssetsParameter()
        {
            Names = new List<string>();
            HashLst = new List<string>();
            ExcludedHashLst = new List<string>();
            AssetLevelType = AssetLevelTypes.ALL;
            IsDefaultWorkingDirectory = null;
        }

        public IEnumerable<string> Names;
        public IEnumerable<string> HashLst { get; set; }
        public IEnumerable<string> ExcludedHashLst { get; set; }
        public AssetLevelTypes AssetLevelType { get; set; }
        public bool? IsDefaultWorkingDirectory { get; set; }
    }
}
