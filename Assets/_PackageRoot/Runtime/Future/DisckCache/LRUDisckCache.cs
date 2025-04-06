using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

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

        string KeyToPath(string key)
        {
            return Path.Combine(DiskCacheFolderPath, KeyHash(key));
        }

        LinkedList<KeyValuePair<string, string>> cache = new();
        Dictionary<string, LinkedListNode<KeyValuePair<string, string>>> nodeCache = new();

        public bool Contains(string key)
        {
            return nodeCache.ContainsKey(key);
        }

        public string Add(string key, byte[] date)
        {
            Debug.Assert(!nodeCache.ContainsKey(key));
            var path = KeyToPath(key);

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllBytes(path, date);

            cache.AddFirst(new KeyValuePair<string, string>(key, path));
            nodeCache.Add(key, cache.First);

            return path;
        }

        public void Remove(string key)
        {
            var path = "";
            if (nodeCache.ContainsKey(key))
            {
                var node = nodeCache[key];
                path = node.Value.Value;

                cache.Remove(node);
                nodeCache.Remove(key);
            }
            else
            {
                path = KeyToPath(key);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public byte[] GetData(string key)
        {
            if (nodeCache.ContainsKey(key))
            {
                var node = nodeCache[key];
                var path = node.Value.Value;
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    cache.Remove(node);
                    cache.AddFirst(node);
                }
                else
                {
                    nodeCache.Remove(key);
                    cache.Remove(node);
                }
            }

            return null;
        }

        public void Clear()
        {
            if (Directory.Exists(DiskCacheFolderPath))
            {
                Directory.Delete(DiskCacheFolderPath, true);
            }
        }
    }
}