using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        public static FutureAudio LoadAudio(string url, bool ignoreImageNotFoundError = false,
            CancellationToken cancellationToken = default)
        {
            var future = new FutureAudio(url, cancellationToken);
            future.StartLoading(ignoreImageNotFoundError);
            return future;
        }
    }
}