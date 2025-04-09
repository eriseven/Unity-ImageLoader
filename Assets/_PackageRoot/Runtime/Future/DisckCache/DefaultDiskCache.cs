using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Extensions.Unity.ImageLoader
{
    internal class DefaultDiskCache : IDiskCache
    {
        private string DiskCacheFolderPath => ImageLoader.settings.diskSaveLocation;

        MD5 _md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        StringBuilder _sb = new System.Text.StringBuilder();

        string KeyHash(string key)
        {
            var ext = Path.GetExtension(key);
            if (!md5Hash)
            {
                return key.GetHashCode() + ext;
            }
                
            lock (_md5)
            {
                var md5Hash = _md5.ComputeHash(Encoding.UTF8.GetBytes(key));
                _sb.Clear();
                for (int i = 0; i < md5Hash.Length; i++)
                {
                    _sb.Append(md5Hash[i].ToString("x2"));
                }

                if (!string.IsNullOrEmpty(ext))
                {
                    _sb.Append(ext);
                }
                return _sb.ToString();
            }
        }

        string KeyToPath(string key)
        {
            return Path.Combine(DiskCacheFolderPath, KeyHash(key));
        }

        private bool md5Hash = true;
        public DefaultDiskCache(bool useMD5 = true)
        {
        }

        public bool Contains(string key)
        {
            return File.Exists(KeyToPath(key));
        }

        public string Add(string key, byte[] date)
        {
            var path = KeyToPath(key);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            File.WriteAllBytes(path, date);
            return path;
        }

        public void Remove(string key)
        {
            var path = KeyToPath(key);
            if (File.Exists(path))
                File.Delete(path);
        }

        public byte[] GetData(string key)
        {
            var path = KeyToPath(key);
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
            else
            {
                return Array.Empty<byte>();
            }
        }

        public void Clear()
        {
            if (Directory.Exists(DiskCacheFolderPath))
            {
                Directory.Delete(DiskCacheFolderPath, true);
            }
        }
    }
    public partial class Settings
    {
        public IDiskCache diskCache = new DefaultDiskCache();
        // public IDiskCache diskCache = new LRUDiskCache(1000);
    }
}