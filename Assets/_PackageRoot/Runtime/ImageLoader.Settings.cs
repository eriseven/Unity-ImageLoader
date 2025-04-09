using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        static Settings _settings;
        /// <summary>
        /// The global settings for the ImageLoader
        /// </summary>
        public static Settings settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = Object.FindObjectOfType<Settings>();
                    if (_settings == null)
                    {
                        _settings = ScriptableObject.CreateInstance<Settings>();
                    }
                }

                return _settings;
            }
        }
    }


    [CreateAssetMenu(fileName = "ImageLoaderSettings", menuName = "ImageLoader/Create Settings")]
    public partial class Settings : ScriptableObject
    {
        /// <summary>
        /// The level of debug messages that will be shown in the console.
        /// Default value is DebugLevel.Warning
        ///
        /// Trace ----- show all messages
        /// Log ------- show only Log, Warning, Error, Exception messages
        /// Warning --- show only Warning, Error, Exception messages
        /// Error ----- show only Error, Exception messages
        /// Exception - show only Exception messages
        /// None ------ show no messages
        /// </summary>
        public DebugLevel debugLevel = DebugLevel.Warning;

        public bool useMemoryCache = true;
#if UNITY_WEBGL
        public bool useDiskCache => false; // default value for WebGL = false
#else
        public bool useDiskCache = true; // default value for non WebGL = true
#endif
        /// <summary>
        /// The location where the images will be saved on disk.
        /// If not set, it will default to UnityEngine.Application.persistentDataPath + "/ImageLoader"
        /// </summary>
        [SerializeField]
        private string _diskSaveLocation = "";

        public string diskSaveLocation
        {
            get => string.IsNullOrEmpty(_diskSaveLocation) ? Application.persistentDataPath + "/ImageLoader" : _diskSaveLocation;
            set { _diskSaveLocation = value; }
        } 

        /// <summary>
        /// The timeout for the web requests
        /// Default value is 30 seconds
        /// </summary>
        [NonSerialized] public TimeSpan timeout = TimeSpan.FromSeconds(30);

        public bool useBaseUrl = false;
        public string baseUrl = "";
    }

    public enum DebugLevel
    {
        Trace = -1, // show all messages
        Log = 0, // show only Log, Warning, Error, Exception messages
        Warning = 1, // show only Warning, Error, Exception messages
        Error = 2, // show only Error, Exception messages
        Exception = 3, // show only Exception messages
        None = 4 // show no messages
    }

    public static class DebugLevelEx
    {
        /// <summary>
        /// Check if the DebugLevel is active
        /// If it is active the related message will be shown in the console
        /// </summary>
        public static bool IsActive(this DebugLevel debugLevel, DebugLevel level) => debugLevel <= level;
    }
}