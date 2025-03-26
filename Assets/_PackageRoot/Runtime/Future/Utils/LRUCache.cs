using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Extensions.Unity.ImageLoader
{
    public interface ILRUCache
    {
        public uint MaxCapacity { get; set; }
        void UpdateItem(string url);
        
        void Remove(string url);
        
        void Clear();

        void CleanUp();
    }

    public partial class Settings
    {
        public ILRUCache lruCache;
    }
    
    public class DummyLRUCache : ILRUCache
    {
        public uint MaxCapacity { get; set; } = 100;
        public void UpdateItem(string url)
        {
        }

        public void Remove(string url)
        {
        }

        public void Clear()
        {
        }

        public void CleanUp()
        {
        }
    }
    

}