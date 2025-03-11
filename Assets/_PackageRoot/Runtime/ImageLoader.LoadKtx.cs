
#if SUPPORT_KTX_TEXTURE
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    public static partial class ImageLoader
    {
        public static FutureKtx LoadKtx(string url, bool mipChain = true, bool ignoreImageNotFoundError = false,
            CancellationToken cancellationToken = default)
        {
            var future = new FutureKtx(url, cancellationToken: cancellationToken);
            future.StartLoading(ignoreImageNotFoundError);
            return future;
        }
    }
}
#endif