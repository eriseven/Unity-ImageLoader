using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Extensions.Unity.ImageLoader.Tests.Utils;
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

            LinkedList<string> cache = new();
            private NodeHash keys = new();

            public uint MaxCapacity { get; set; } = 100;
            
            public int Count => cache.Count;
            
            public string First => cache.First.Value;
            
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
            var cache = new LRUCache();
            ImageLoader.settings.lruCache = cache;
            
            foreach (var imageURL in TestUtils.ImageURLs)
            {
                yield return LoadSprite(imageURL).TimeoutCoroutine(TimeSpan.FromSeconds(10));
                Assert.IsTrue(ImageLoader.DiskCacheContains(imageURL));
            }

            var lastUrl = TestUtils.ImageURLs[0];
            yield return LoadSprite(lastUrl).TimeoutCoroutine(TimeSpan.FromSeconds(10));
            
            // Debug.Log($"{cache.First} : /_{lastUrl}{Path.GetExtension(lastUrl)}");
            Assert.IsTrue(cache.First.Contains($"/_{lastUrl.GetHashCode()}{Path.GetExtension(lastUrl)}"));
            
            cache.MaxCapacity = 2;
            yield return LoadSprite(lastUrl).TimeoutCoroutine(TimeSpan.FromSeconds(10));
            Assert.IsTrue(cache.First.Contains($"/_{lastUrl.GetHashCode()}{Path.GetExtension(lastUrl)}"));
            Assert.IsTrue(cache.Count == 2);
            
        }
    }
}