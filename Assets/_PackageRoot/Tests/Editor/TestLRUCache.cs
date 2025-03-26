using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Extensions.Unity.ImageLoader.Tests.Utils;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestLRUCache : TestCache
    {
        [Serializable]
        public class LRUCache : ILRUCache
        {
            internal class NodeHash : KeyedCollection<string, LinkedListNode<string>>
            {
                protected override string GetKeyForItem(LinkedListNode<string> item)
                {
                    return item.Value;
                }
            }

            [JsonProperty]
            LinkedList<string> cache = new();
            private NodeHash keys = new();

            public uint MaxCapacity { get; set; } = 100;

            [JsonIgnore]
            public int Count => cache.Count;

            [JsonIgnore]
            public string First => cache.First.Value;

            [JsonIgnore]
            public string Last => cache.Last.Value;

            public void UpdateItem(string url)
            {
                if (keys.Contains(url))
                {
                    cache.Remove(keys[url]);
                    cache.AddFirst(keys[url]);

                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[LRUCache]Item({url} updated.)");
                }
                else
                {
                    cache.AddFirst(url);
                    keys.Add(cache.First);
                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[LRUCache]Item({url} added.)");
                }

                CheckCapacity();
            }

            public void Remove(string url)
            {
                if (keys.Contains(url))
                {
                    cache.Remove(keys[url]);
                    keys.Remove(url);
                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[LRUCache]Item({url} removed.)");
                }
            }

            public void Clear()
            {
                cache.Clear();
                keys.Clear();
                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                    Debug.Log($"[LRUCache] cleared.)");
            }

            public void CleanUp()
            {
            }

            public void Init()
            {
                keys.Clear();
                var node = cache.First;
                while (node != null)
                {
                    keys.Add(node);
                    node = node.Next;
                }
            }

            void CheckCapacity()
            {
                if (cache.Count <= MaxCapacity)
                {
                    return;
                }

                if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                    Debug.Log($"[LRUCache]CheckCapacity.)");

                while (cache.Count > MaxCapacity)
                {
                    var node = cache.Last;
                    cache.RemoveLast();
                    keys.Remove(node.Value);
                    if (File.Exists(node.Value))
                    {
                        try
                        {
                            File.Delete(node.Value);
                        }
                        catch (Exception e)
                        {
                            if (!ImageLoader.settings.debugLevel.IsActive(DebugLevel.None))
                                Debug.LogException(e);
                        }
                    }

                    if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[LRUCache]Item({node.Value}) removed.)");
                }
            }
        }

        LRUCache LoadLRUCache()
        {
            LRUCache cache;
            var path = Path.Combine(ImageLoader.settings.diskSaveLocation, "lru_cache.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                cache = JsonConvert.DeserializeObject<LRUCache>(json,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
            }
            else
            {
                cache = new LRUCache();
            }

            cache.Init();
            return cache;
        }

        void SaveLRUCache(LRUCache cache)
        {
            var path = Path.Combine(ImageLoader.settings.diskSaveLocation, "lru_cache.json");
            var json = JsonConvert.SerializeObject(cache, Formatting.Indented,
                new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[LRUCache] saved. \n{json})");

            File.WriteAllText(path, json);
        }

        [UnitySetUp]
        public override IEnumerator SetUp()
        {
            ImageLoader.settings.lruCache = new LRUCache();
            return base.SetUp();
        }

        [UnityTest]
        public IEnumerator TestLRUCDiskache()
        {
            if (Directory.Exists(ImageLoader.settings.diskSaveLocation))
                Directory.Delete(ImageLoader.settings.diskSaveLocation, true);

            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;
            
            var path = Path.Combine(ImageLoader.settings.diskSaveLocation, "lru_cache.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            var cache = LoadLRUCache();
            ImageLoader.settings.lruCache = cache;

            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).TimeoutCoroutine(TimeSpan.FromSeconds(10));
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
            }

            SaveLRUCache(cache);
            cache = LoadLRUCache();
            ImageLoader.settings.lruCache = cache;

            var lastUrl = TestUtils.ImageURLs[0];
            yield return LoadSprite(lastUrl).TimeoutCoroutine(TimeSpan.FromSeconds(10));
            Assert.IsTrue(cache.First.Contains($"/_{lastUrl.GetHashCode()}{Path.GetExtension(lastUrl)}"));
            
            SaveLRUCache(cache);
            cache = LoadLRUCache();
            ImageLoader.settings.lruCache = cache;
            Assert.IsTrue(cache.First.Contains($"/_{lastUrl.GetHashCode()}{Path.GetExtension(lastUrl)}"));

            cache.MaxCapacity = 2;
            yield return LoadSprite(lastUrl).TimeoutCoroutine(TimeSpan.FromSeconds(10));
            Assert.IsTrue(cache.First.Contains($"/_{lastUrl.GetHashCode()}{Path.GetExtension(lastUrl)}"));
            Assert.IsTrue(cache.Count == 2);
            
            SaveLRUCache(cache);
            cache = LoadLRUCache();
            ImageLoader.settings.lruCache = cache;
            
            Assert.IsTrue(cache.First.Contains($"/_{lastUrl.GetHashCode()}{Path.GetExtension(lastUrl)}"));
            Assert.IsTrue(cache.Count == 2);
 
        }
    }
}