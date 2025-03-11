#if SUPPORT_KTX_TEXTURE
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Extensions.Unity.ImageLoader.Tests
{
    public class TestLoadingKtx
    {
        public static readonly string[] KtxImageURLs =
        {
            "https://github.com/eriseven/Unity-ImageLoader/raw/refs/heads/develop/Test%20KtxImages/3d.ktx2",
        };

        async UniTask LoadKtxAsync(string url)
        {
            var txture = await ImageLoader.LoadKtx(url);
            Assert.IsNotNull(txture);
        }

        [UnityTest]
        public IEnumerator LoadKtxFromUrl()
        {
            ImageLoader.settings.useDiskCache = false;
            ImageLoader.settings.useMemoryCache = false;

            foreach (var url in KtxImageURLs)
            {
                yield return LoadKtxAsync(url).ToCoroutine();
            }
        }
    }
}
#endif