namespace Extensions.Unity.ImageLoader
{
    public interface IDiskCache
    {
        bool Contains(string key, out string path);
        string Add(string key, byte[]date);
        void Remove(string key);
        byte[] GetData(string key);
        void Clear();
    }
}