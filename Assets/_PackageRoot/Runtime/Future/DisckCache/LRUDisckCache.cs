using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Extensions.Unity.ImageLoader
{
    public class LRUDisckCache : IDiskCache
    {
        private string DiskCacheFolderPath => ImageLoader.settings.diskSaveLocation;

        MD5 _md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        StringBuilder _sb = new System.Text.StringBuilder();

        string KeyHash(string key)
        {
            var ext = Path.GetExtension(key);

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
        
        public bool Contains(string key)
        {
            throw new System.NotImplementedException();
        }

        public string Add(string key, byte[] date)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new System.NotImplementedException();
        }

        public byte[] GetData(string key)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }
    }
}