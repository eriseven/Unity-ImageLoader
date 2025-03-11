using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        public static IFuture<Reference<AudioClip>> LoadAudioRef(string url, bool ignoreImageNotFoundError = false,
            CancellationToken cancellationToken = default)
        {
            var future = new FutureAudio(url, cancellationToken);
            var futureRef = future.AsReference(settings.debugLevel);
            
            future.StartLoading(ignoreImageNotFoundError);
            
            return futureRef;
        }
        
        public static void ClearAudioRef() => Reference<AudioClip>.Clear();
        
        public static bool ClearAudioRef(string url) => Reference<AudioClip>.Clear(url);
    }
}