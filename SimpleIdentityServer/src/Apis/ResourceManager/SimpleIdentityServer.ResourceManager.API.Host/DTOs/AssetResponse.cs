using Newtonsoft.Json.Linq;
using System;

namespace SimpleIdentityServer.ResourceManager.API.Host.DTOs
{
    internal sealed class AssetSecurity
    {
        public AssetSecurity(bool read, bool write, bool locked, bool hasSecurity)
        {
            Read = read;
            Write = write;
            Locked = locked;
            HasSecurity = hasSecurity;
        }

        public bool Read { get; private set; }
        public bool Write { get; private set; }
        public bool Locked { get; private set; }
        public bool HasSecurity { get; private set; }
    }

    internal sealed class AssetResponse // https://github.com/Studio-42/elFinder/wiki/Client-Server-API-2.0#information-about-filedirectory
    {
        private AssetResponse(string name, string hash)
        {
            Name = name;
            Hash = hash;
        }

        public static AssetResponse Create(string name, string hash, string volumeId, bool containsChildren, string pHash, string mimeType, AssetSecurity assetSecurity)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(hash))
            {
                throw new ArgumentNullException(nameof(hash));
            }

            if (string.IsNullOrWhiteSpace(volumeId))
            {
                throw new ArgumentNullException(nameof(volumeId));
            }

            if (string.IsNullOrWhiteSpace(mimeType))
            {
                throw new ArgumentNullException(nameof(mimeType));
            }

            if (assetSecurity == null)
            {
                throw new ArgumentNullException(nameof(assetSecurity));
            }

            var result = new AssetResponse(name, hash);
            result.Dirs = (containsChildren) ? 1: 0;
            result.Read = (assetSecurity.Read) ? 1 : 0;
            result.Write = (assetSecurity.Write) ? 1 : 0;
            result.Locked = (assetSecurity.Locked) ? 1 : 0;
            result.HasSecurity = (assetSecurity.HasSecurity) ? 1 : 0;
            result.VolumeId = volumeId;
            result.Phash = pHash;
            result.Mime = mimeType;
            return result;
        }

        public static AssetResponse CreateFile()
        {
            return null;
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
        /// (Number) has authorization policies.
        /// </summary>
        public int HasSecurity { get; set; }
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
            result.Add(Constants.ElFinderCwdResponseNames.HasSecurity, HasSecurity);
            result.Add(Constants.ElFinderCwdResponseNames.Locked, Locked);
            result.Add(Constants.ElFinderCwdResponseNames.Phash, Phash);
            result.Add(Constants.ElFinderCwdResponseNames.VolumeId, VolumeId);
            result.Add(Constants.ElFinderCwdResponseNames.Mime, Mime);
            return result;
        }
    }
}
