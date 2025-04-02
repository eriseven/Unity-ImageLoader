using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cysharp.Threading.Tasks;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extensions.Unity.ImageLoader
{
    public static class UObjectExtensions
    {
        public static void Release(this Object obj, DebugLevel logLevel = DebugLevel.Log)
        {
            if (PlayerLoopHelper.IsMainThread)
            {
                if (obj.IsNull())
                    return;

                if (logLevel.IsActive(DebugLevel.Trace))
                    Debug.Log($"[ImageLoader] Release memory Texture2D");
                UnityEngine.Object.DestroyImmediate(obj);
                return;
            }

            UniTask.Post(() =>
            {
                if (obj.IsNull()) // double check after async delay
                    return;

                if (logLevel.IsActive(DebugLevel.Trace))
                    Debug.Log($"[ImageLoader] Release memory Texture2D");
                UnityEngine.Object.DestroyImmediate(obj);
            });
        }

        public static void Release(this Sprite sprite, DebugLevel logLevel = DebugLevel.Log)
        {
            if (PlayerLoopHelper.IsMainThread)
            {
                if (sprite.IsNull())
                    return;

                if (sprite.texture.IsNotNull())
                {
                    if (logLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Release memory Sprite->Texture2D");
                    UnityEngine.Object.DestroyImmediate(sprite.texture);
                }

                UnityEngine.Object.DestroyImmediate(sprite);
                return;
            }

            UniTask.Post(() =>
            {
                if (sprite.IsNull()) // double check after async delay
                    return;

                if (sprite.texture.IsNotNull()) // double check after async delay
                {
                    if (logLevel.IsActive(DebugLevel.Trace))
                        Debug.Log($"[ImageLoader] Release memory Sprite->Texture2D");
                    UnityEngine.Object.DestroyImmediate(sprite.texture);
                }

                UnityEngine.Object.DestroyImmediate(sprite);
            });
        }

        public static void Release<T>(this FutureReference<T> futureReference, DebugLevel logLevel = DebugLevel.Log) where T : UnityEngine.Object
        {
            throw new Exception($"Can not release {futureReference}");
        }
    }

    public static class MemoryCacheExtensions
    {
        public static void RemoveAndRelease<T>(this IMemoryCache cache, string key, DebugLevel logLevel = DebugLevel.Log) where T : Object
        {
            T obj = cache.Get(key, typeof(T)) as T;
            if (obj.IsNotNull())
            {
                cache.Remove(key, typeof(T), true, logLevel);
            }
        }

        public static T Get<T>(this IMemoryCache cache, string key, DebugLevel logLevel = DebugLevel.Log) where T : Object
        {
            return cache.Get(key, typeof(T), logLevel) as T;
        }
    }

    public class DefaultMemoryCache : IMemoryCache
    {
        private Dictionary<Type, Dictionary<string, Object>> typeMap = new();

        public bool Contains(string key, Type type, DebugLevel logLevel = DebugLevel.Log)
        {
            return typeMap.TryGetValue(type, out var cache) && cache.ContainsKey(key);
        }

        public void Add(string key, Object obj, DebugLevel logLevel = DebugLevel.Log)
        {
            Debug.Assert(obj.IsNotNull());

            if (!typeMap.TryGetValue(obj.GetType(), out var cache))
            {
                cache = new Dictionary<string, Object>();
                typeMap.Add(obj.GetType(), cache);
            }

            if (cache.TryGetValue(key, out var value))
            {
                Debug.LogError($"[DefaultMemoryCache] key:{key} already exists! ");
            }
            else
            {
                cache.Add(key, obj);
            }
        }

        public void Remove(string key, Type type, bool destroy = true, DebugLevel logLevel = DebugLevel.Log)
        {
            if (typeMap.TryGetValue(type, out var cache) && cache.TryGetValue(key, out var obj))
            {
                cache.Remove(key);
                if (destroy)
                {
                    obj?.Release();
                }
            }
            else
            {
                Debug.LogError($"[DefaultMemoryCache] Remove key:{key}  not found.");
            }
        }

        public void RemoveAllType(string key, bool destroy = true, DebugLevel logLevel = DebugLevel.Log)
        {
            foreach (var cache in typeMap.Values)
            {
                if (cache.TryGetValue(key, out var obj))
                {
                    cache.Remove(key);
                    if (destroy)
                    {
                        obj?.Release();
                    }
                }
            }
        }

        public Object Get(string key, Type type, DebugLevel logLevel = DebugLevel.Log)
        {
            if (typeMap.TryGetValue(type, out var cache) && cache.TryGetValue(key, out var obj))
            {
                return obj;
            }

            Debug.LogWarning($"[DefaultMemoryCache] Get key {key} not found");
            return null;
        }

        public void Clear(Type type, bool destroy = true, DebugLevel logLevel = DebugLevel.Log)
        {
            if (typeMap.TryGetValue(type, out var cache))
            {
                typeMap.Remove(type);
                if (destroy)
                {
                    foreach (var obj in cache.Values)
                    {
                        obj?.Release();
                    }
                }
            }
        }

        public void Clear(bool destroy = true, DebugLevel logLevel = DebugLevel.Log)
        {
            if (destroy)
                foreach (var cache in typeMap.Values)
                {
                    foreach (var obj in cache.Values)
                    {
                        obj?.Release();
                    }
                }

            typeMap.Clear();
        }
    }

    public partial class Settings
    {
        public IMemoryCache memoryCache = new DefaultMemoryCache();
    }
}