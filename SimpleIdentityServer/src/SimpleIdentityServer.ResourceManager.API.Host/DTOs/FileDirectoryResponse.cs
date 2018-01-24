using Newtonsoft.Json.Linq;
using System;

namespace SimpleIdentityServer.ResourceManager.API.Host.DTOs
{
    internal sealed class FileDirectoryResponse // https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#information-about-filedirectory
    {
        private FileDirectoryResponse(string name, string hash)
        {
            Name = name;
            Hash = hash;
        }

        public static FileDirectoryResponse CreateRootFolder(string name, string hash, string volumeId)
        {
            var result = new FileDirectoryResponse(name, hash);
            result.Dirs = 1;
            result.Read = 1;
            result.Write = 0;
            result.Write = 1;
            result.Locked = 1;
            result.VolumeId = volumeId;
            result.Mime = "directory";
            return result;
        }

        public static FileDirectoryResponse CreateFolder(string name, string hash, string pHash)
        {
            var result = new FileDirectoryResponse(name, hash);
            result.Dirs = 0;
            result.Read = 1;
            result.Write = 1;
            result.Write = 1;
            result.Phash = pHash;
            return result;
        }

        /// <summary>
        /// (String) name of file/dir. Required
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// (String) hash of current file/dir path, first symbol must be letter, symbols before _underline_ - volume id, Required. 
        /// </summary>
        public string Hash { get; private set; }
        /// <summary>
        /// (String) hash of parent directory. Required except roots dirs.
        /// </summary>
        public string Phash { get; private set; }
        /// <summary>
        /// (String) mime type. Required.
        /// </summary>
        public string Mime { get; private set; }
        /// <summary>
        /// (Number) file modification time in unix timestamp. Required.
        /// </summary>
        public int Ts { get; private set; }
        /// <summary>
        /// (String) last modification time (mime). Depricated but yet supported. Use ts instead.
        /// </summary>
        public DateTime Date { get; private set; }
        /// <summary>
        /// (Number) file size in bytes
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// (Number) Only for directories. Marks if directory has child directories inside it. 0 (or not set) - no, 1 - yes. Do not need to calculate amount.
        /// </summary>
        public int Dirs { get; private set; }
        /// <summary>
        /// (Number) is readable
        /// </summary>
        public int Read { get; private set; }
        /// <summary>
        /// (Number) is writable
        /// </summary>
        public int Write { get; private set; }
        /// <summary>
        /// (Number) is file locked. If locked that object cannot be deleted,  renamed or moved
        /// </summary>
        public int Locked { get; private set; }
        /// <summary>
        /// (String) Only for images. Thumbnail file name, if file do not have thumbnail yet, but it can be generated than it must have value "1"
        /// </summary>
        public string Tmb { get; private set; }
        /// <summary>
        /// (String) For symlinks only. Symlink target path.
        /// </summary>
        public string Alias { get; private set; }
        /// <summary>
        /// (String) For symlinks only. Symlink target hash.
        /// </summary>
        public string THash { get; private set; }
        /// <summary>
        /// (String) For images - file dimensions. Optionally.
        /// </summary>
        public string Dim { get; private set; }
        /// <summary>
        /// (String) Volume id. For root dir only.
        /// </summary>
        public string VolumeId { get; private set; }

        public JObject GetJson()
        {
            var result = new JObject();
            result.Add(Constants.ElFinderCwdResponseNames.Name, Name);
            result.Add(Constants.ElFinderCwdResponseNames.Hash, Hash);
            result.Add(Constants.ElFinderCwdResponseNames.Dirs, Dirs);
            result.Add(Constants.ElFinderCwdResponseNames.Read, Read);
            result.Add(Constants.ElFinderCwdResponseNames.Write, Write);
            result.Add(Constants.ElFinderCwdResponseNames.Locked, Locked);
            result.Add(Constants.ElFinderCwdResponseNames.Phash, Phash);
            result.Add(Constants.ElFinderCwdResponseNames.VolumeId, VolumeId);
            result.Add(Constants.ElFinderCwdResponseNames.Mime, Mime);
            return result;
        }
    }
}
