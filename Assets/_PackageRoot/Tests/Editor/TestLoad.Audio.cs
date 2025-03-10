using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestLoadAudio
    {
        static readonly string[] AudioURLs =
        {
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.aiff",
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.mp3",
            "https://github.com/IvanMurzak/Unity-AudioLoader/raw/master/Test%20Audio%20Files/sample.wav"
        };

        async UniTask LoadAudioClipAsync(string url)
        {
            var audioClip = await ImageLoader.LoadAudio(url);
            Assert.IsNotNull(audioClip);
        }

        [UnityTest]
        public IEnumerator LoadAudioFromUrl()
        {
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in AudioURLs)
            {
                yield return LoadAudioClipAsync(url).ToCoroutine();
            }
        }

        [UnityTest]
        public IEnumerator LoadAudioCacheDisk()
        {
            ImageLoader.settings.useDiskCache = true;
            ImageLoader.settings.useMemoryCache = false;
            foreach (var url in AudioURLs)
            {
                yield return LoadAudioClipAsync(url).ToCoroutine();
            }
        }
    }
}