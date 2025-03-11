using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        public static IFuture<Reference<Texture2D>> LoadKtxRef(string url, bool mipChain = true,
            bool ignoreImageNotFoundError = false,
            CancellationToken cancellationToken = default)
        {
            var future = new FutureKtx(url, cancellationToken: cancellationToken);
            var futureRef = future.AsReference();
            future.StartLoading(ignoreImageNotFoundError);
            return futureRef;
        }
    }
}