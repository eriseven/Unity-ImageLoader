using UnityEngine;
using Gilzoide.EasyProjectSettings;

namespace Extensions.Unity.ImageLoader
{
    [ProjectSettings("Assets/ImageLoader/Settings", SettingsPath = "Project/ImageLoader/Settings")]
    public class SettingsAsset : ScriptableObject
    {
        [SerializeField]
        public Settings settings;
    }
}