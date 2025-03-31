using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public partial class Future<T>
    {
        internal static IDiskCache diskCache => ImageLoader.settings.diskCache;
        
        internal static readonly TaskFactory diskTaskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));

        protected static string DiskCacheFolderPath => $"{ImageLoader.settings.diskSaveLocation}/_{typeof(T).Name}";
        protected static string DiskCachePath(string url) => $"{DiskCacheFolderPath}/_{url.GetHashCode()}{Path.GetExtension(url)}";

        protected static void SaveDisk(string url, byte[] data)
        {
            diskCache.Add(url, data);
        }
        protected static byte[] LoadDisk(string url)
        {
            return diskCache.GetData(url);
        }
        protected static Task SaveDiskAsync(string url, byte[] data, DebugLevel logLevel)
        {
            if (logLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Save to Disk cache ({typeof(T).Name})\n{url}");
            return diskTaskFactory.StartNew(() => SaveDisk(url, data));
        }
        protected static Task<byte[]> LoadDiskAsync(string url, DebugLevel logLevel)
        {
            if (logLevel.IsActive(DebugLevel.Trace))
                Debug.Log($"[ImageLoader] Load from Disk cache ({typeof(T).Name})\n{url}");
            return diskTaskFactory.StartNew(() => LoadDisk(url));
        }

        /// <summary>
        /// Check if the image is cached at Disk
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if image is cached at Disk</returns>
        // public static bool DiskCacheContains(string url) => File.Exists(DiskCachePath(url));
        public static bool DiskCacheContains(string url) => diskCache.Contains(url);

        /// <summary>
        /// Check if the image is cached at Disk
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <returns>Returns true if image is cached at Disk</returns>
        public static Task<bool> DiskCacheExistsAsync(string url)
        {
            // var path = DiskCachePath(url);
            // return diskTaskFactory.StartNew(() => File.Exists(path));
            return diskTaskFactory.StartNew(() => DiskCacheContains(url));
        }

        /// <summary>
        /// Save sprite to Disk cache directly. Should be used for overloading cache system
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        /// <param name="obj">object which should be saved</param>
        /// <param name="replace">replace existed cached sprite if any</param>
        public static Task SaveToDiskCache(string url, byte[] obj, bool replace = false, DebugLevel logLevel = DebugLevel.Error)
        {
            if (!replace && DiskCacheContains(url))
            {
                if (logLevel.IsActive(DebugLevel.Warning))
                    Debug.LogError($"[ImageLoader] Can't set to Disk cache ({typeof(T).Name}), because it already contains the key. Use 'replace = true' to replace\n{url}");
                return Task.CompletedTask;
            }
            return SaveDiskAsync(url, obj, logLevel);
        }

        /// <summary>
        /// Clear Disk cache for all urls
        /// </summary>
        public static Task ClearDiskCache()
        {
            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Clear Disk cache ({typeof(T).Name}) All");
            return diskTaskFactory.StartNew(() => diskCache.Clear());
        }

        /// <summary>
        /// Clear Disk cache for the given url
        /// </summary>
        /// <param name="url">URL to the picture, web or local</param>
        public static Task ClearDiskCache(string url)
        {
            if (ImageLoader.settings.debugLevel.IsActive(DebugLevel.Log))
                Debug.Log($"[ImageLoader] Clear Disk cache ({typeof(T).Name})\n{url}");
            return diskTaskFactory.StartNew(() => diskCache.Remove(url));
        }
    }
}
