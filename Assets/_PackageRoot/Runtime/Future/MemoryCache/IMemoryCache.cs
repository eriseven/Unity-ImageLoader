using System;

namespace Extensions.Unity.ImageLoader
{
    public interface IMemoryCache
    {
        bool Contains(string key, Type type, DebugLevel logLevel = DebugLevel.Log);
        void Add(string key, UnityEngine.Object obj, DebugLevel logLevel = DebugLevel.Log);
        void Remove(string key, Type type, bool destroy = true, DebugLevel logLevel = DebugLevel.Log);
        void RemoveAllType(string key, bool destroy = true, DebugLevel logLevel = DebugLevel.Log);
        UnityEngine.Object Get(string key, Type type, DebugLevel logLevel = DebugLevel.Log);
        
        void Clear(Type type, bool destroy = true, DebugLevel logLevel = DebugLevel.Log);
        void Clear(bool destroy = true, DebugLevel logLevel = DebugLevel.Log);
    }
}