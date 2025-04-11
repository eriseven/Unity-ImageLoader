using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    [Serializable]
    internal struct AudioInfo
    {
        public string name;
        public int lengthSamples;
        public int channels;
        public int frequency;
        public byte[] data;
    }
    
    public class FutureAudio : Future<AudioClip>
    {
        public FutureAudio(string url, CancellationToken cancellationToken = default, DebugLevel? logLevel = null) :
            base(url, cancellationToken, logLevel)
        {
        }

        protected override Task<AudioClip> ParseWebRequest(UnityWebRequest webRequest)
        {
            return Task.FromResult((webRequest.downloadHandler as DownloadHandlerAudioClip)?.audioClip);
        }

        protected override UnityWebRequest CreateWebRequest(string url)
        {
            return UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
        }

        protected override void ReleaseMemory(AudioClip obj, DebugLevel logLevel = DebugLevel.Log)
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

            // if (obj.IsNull())
            //     return;

            UniTask.Post(() =>
            {
                if (obj.IsNull()) // double check after async delay
                    return;

                if (logLevel.IsActive(DebugLevel.Trace))
                    Debug.Log($"[ImageLoader] Release memory Texture2D");
                UnityEngine.Object.DestroyImmediate(obj);
            });
        }

        protected override async Task<AudioClip> LoadFromDiskAsync()
        {
            diskCache.Contains(Url, out var path);
            var url = "file:///" + (path);
            var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
            await request.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
            var isError = request.result != UnityEngine.Networking.UnityWebRequest.Result.Success;
#else
            var isError = request.isNetworkError || WebRequest.isHttpError;
#endif
            if (isError)
            {
                return null;
            }
            
            return (request.downloadHandler as DownloadHandlerAudioClip)?.audioClip;
        }
        
        protected override AudioClip ParseBytes(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
    }
}