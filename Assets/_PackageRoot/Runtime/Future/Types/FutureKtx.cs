#if SUPPORT_KTX_TEXTURE
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using KtxUnity;
using UnityEngine;
using UnityEngine.Networking;

namespace Extensions.Unity.ImageLoader
{
    public class FutureKtx : Future<Texture2D>
    {
        // public FutureKtx(string url, CancellationToken cancellationToken = default, DebugLevel? logLevel = null) : base(url, cancellationToken, logLevel)
        protected bool mipChain;

        public FutureKtx(string url, bool mipChain = true,
            CancellationToken cancellationToken = default) : base(url, cancellationToken)
        {
            this.mipChain = mipChain;
        }

        protected override async Task<Texture2D> LoadFromDiskAsync()
        {
            var url = "file:///" + (DiskCachePath(Url));
            var request = CreateWebRequest(url);
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
            
            return await ParseWebRequest(request);
        }
        
        protected override async Task<Texture2D> ParseWebRequest(UnityWebRequest webRequest)
        {
            var buffer = webRequest.downloadHandler.data;


            var ktx = new KtxTexture();

            using (var bufferWrapped = new ManagedNativeArray(buffer))
            {
                var result = await ktx.LoadFromBytes(data: bufferWrapped.nativeArray, mipChain: mipChain);
                return result.texture;
            }

            return null;
        }

        protected override UnityWebRequest CreateWebRequest(string url)
        {
            return UnityWebRequest.Get(url);
        }

        protected override void ReleaseMemory(Texture2D obj, DebugLevel logLevel = DebugLevel.Log)
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

        protected override Texture2D ParseBytes(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
    }
}
#endif