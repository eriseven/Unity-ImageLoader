using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public class LRUDiskCache : IDiskCache
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

        [Serializable]
        public struct CacheEntry
        {
            public string Key;
            public string Path;
        }
        
        LinkedList<CacheEntry> cache = new();
        Dictionary<string, LinkedListNode<CacheEntry>> nodeCache = new();

        private int maxCacheSize { get; set; } = 100;

        private DateTime lastStoreTime;
        public LRUDiskCache(int maxSize)
        {
            lastStoreTime = DateTime.Now;
            maxCacheSize = maxSize;
        }
        
        public bool Contains(string key)
        {
            if (nodeCache.TryGetValue(key, out var node))
            {
                if (File.Exists(node.Value.Path))
                {
                    cache.Remove(node);
                    cache.AddFirst(node);
                    return true;
                }
                
                Remove(key);
                return false;
            }

            var path = KeyToPath(key);
            if (File.Exists(path))
            {
                cache.AddFirst(new CacheEntry { Key = key, Path = path });
                nodeCache.Add(key, cache.First);
                return true;
            }
            return false;
        }

        void RemoveLast()
        {
            var last = cache.Last;
            if (last != null)
            {
                if (File.Exists(last.Value.Path))
                {
                    File.Delete(last.Value.Path);
                }
                
                nodeCache.Remove(last.Value.Key);
                cache.RemoveLast();
            }
        }
        
        
        public string Add(string key, byte[] date)
        {
            if (nodeCache.TryGetValue(key, out var node))
            {
                Debug.LogWarning($"[ImageLoader] Duplicate key: {key}");
                return node.Value.Path;
            }
            
            var path = KeyToPath(key);

            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllBytes(path, date);

            cache.AddFirst(new CacheEntry { Key = key, Path = path });
            nodeCache.Add(key, cache.First);

            while (cache.Count > maxCacheSize)
            {
                RemoveLast();
            }

            return path;
        }

        public void Remove(string key)
        {
            var path = "";
            if (nodeCache.ContainsKey(key))
            {
                var node = nodeCache[key];
                path = node.Value.Path;

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
                var path = node.Value.Path;
                if (File.Exists(path))
                {
                    var bytes = File.ReadAllBytes(path);
                    cache.Remove(node);
                    cache.AddFirst(node);
                    return bytes;
                }

                nodeCache.Remove(key);
                cache.Remove(node);
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

        void StoreCache()
        {
            var ts = DateTime.Now - lastStoreTime;
            if (ts.TotalMinutes >= 2)
            {
                var jsonString = JsonUtility.ToJson(cache.ToList());
            }
        }
    }
}