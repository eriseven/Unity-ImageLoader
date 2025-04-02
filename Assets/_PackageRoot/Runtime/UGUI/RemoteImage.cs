using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader.UGUI
{
    public class RemoteImage : Image
    {
        [SerializeField] private string m_url = "";

        public string url
        {
            get => m_url;
            set
            {
                if (m_url != value)
                {
                    m_url = value;
                    Dirty = true;
                    Reload();
                }
            }
        }


        // IFuture<Reference<Sprite>> future;
        IFuture<Reference<Texture2D>> future;
        Reference<Texture2D> textrueRef;

        [SerializeField]
        private Texture2D placeHoder;

        private Sprite m_PlaceHoderSprite;

        private Sprite placeHoderSprite
        {
            get
            {
                if (m_PlaceHoderSprite.IsNull() && placeHoder.IsNotNull())
                {
                    m_PlaceHoderSprite = placeHoder.ToSprite();
                }
                return m_PlaceHoderSprite;
            }
        }

        private bool dirty = true;

        void OnLoaded(Reference<Texture2D> reference)
        {
            Debug.Log("RemoteImage.Reload() Loaded");
            textrueRef = reference;
        }

        void OnFailed(Exception exception)
        {
            Debug.Log("RemoteImage.Reload() failed");
            if (overrideSprite == placeHoderSprite)
            {
                overrideSprite = null;
            }
            future?.Dispose();
            future = null;
        }

        void Reload()
        {
            if (dirty || overrideSprite == null)
            {
                if (overrideSprite == null && textrueRef is { IsDisposed: false })
                {
                    overrideSprite = textrueRef.Value.ToSprite();
                }
                
                if (future != null && future.Url == m_url)
                {
                    return;
                }

                Debug.Log("RemoteImage.Reload()");
                if (!string.IsNullOrEmpty(url))
                {
                    overrideSprite = placeHoderSprite;
                    textrueRef?.Dispose();
                    textrueRef = null;

                    future?.Dispose();
                    future = null;
                    dirty = false;
                    future = ImageLoader.LoadTextureRef(url)
                        .Loaded(OnLoaded)
                        .Failed(OnFailed)
                        .Consume(this)
                        .Canceled(() => { Debug.Log("RemoteImage.Reload() cancelled"); });
                }
                else
                {
                    overrideSprite = null;
                }
            }
        }

        protected override void OnEnable()
        {
            Debug.Log("RemoteImage.OnEnable()");
            base.OnEnable();
            Reload();
        }

        protected override void OnDestroy()
        {
            Debug.Log("RemoteImage.OnDestroy()");
            base.OnDestroy();
            future?.Dispose();
            textrueRef?.Dispose();
        }

        public bool Dirty
        {
            set
            {
                dirty = value;
                if (dirty)
                {
                    if (future != null && !future.IsCompleted)
                    {
                        future.Cancel();
                        future.Dispose();
                        future = null;
                    }
                }
            }
        }

#if UNITY_EDITOR


        protected override void OnValidate()
        {
            Debug.Log("RemoteImage.OnValidate()");
            base.OnValidate();
            Reload();
        }
#endif
    }
}