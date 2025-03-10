using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        /// <summary>
        /// Check if the image is cached at Disk
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if image is cached at Disk</returns>
        protected virtual bool DiskCacheContains() => DiskCacheContains(Url);
        protected virtual Task SaveDiskAsync(byte[] data, T obj = default(T))
        {
            if (LogLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Save to Disk cache ({typeof(T).Name})\n{Url}");
            return SaveDiskAsync(Url, data, LogLevel);
        }
        protected virtual Task<byte[]> LoadDiskAsync() => LoadDiskAsync(Url, LogLevel);
        protected abstract T ParseBytes(byte[] bytes);

        protected virtual async Task<T> LoadFromDiskAsync()
        {
            var bytes = await LoadDiskAsync();
            if (bytes is { Length: > 0 })
            {
                await UniTask.SwitchToMainThread();
                if (IsCancelled || Status == FutureStatus.FailedToLoad)
                {
                    return default(T);
                }
                return ParseBytes(bytes);
            }

            return default(T);
        }
    }
}
