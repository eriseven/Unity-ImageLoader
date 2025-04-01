using System;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader.UGUI
{
    public class RemoteRawImage : RawImage
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

        private bool dirty = true;

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

        IFuture<Reference<Texture2D>> future;
        Reference<Texture2D> textrueRef;

        void OnLoaded(Reference<Texture2D> reference)
        {
            Debug.Log("RemoteRawImage.Reload() Loaded");
            reference.DisposeOnDisable(this);
            remoteTexture = reference.Value;
            textrueRef = reference;
            SetVerticesDirty();
            SetMaterialDirty();
        }

        void OnFailed(Exception exception)
        {
            Debug.Log("RemoteRawImage.Reload() failed");
            future?.Dispose();
            future = null;
        }

        void Reload()
        {
            if (dirty || remoteTexture == null)
            {
                if (future != null && future.Url == m_url)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(url))
                {
                    remoteTexture = null;
                    textrueRef?.Dispose();
                    textrueRef = null;

                    future?.Dispose();
                    future = null;
                    dirty = false;
                    future = ImageLoader.LoadTextureRef(url)
                        .Loaded(OnLoaded)
                        .Failed(OnFailed)
                        .CancelOnDisable(this)
                        .Canceled(() => { Debug.Log("RemoteRawImage.Reload() cancelled"); });

                    future.Forget();
                }
            }
        }

        protected override void OnEnable()
        {
            Debug.Log("RemoteRawImage.OnEnable()");
            base.OnEnable();
            Reload();
        }

        protected override void OnDisable()
        {
            Debug.Log("RemoteRawImage.OnDisable()");

            remoteTexture = null;
            base.OnDisable();
            future = null;

            textrueRef?.Dispose();
            textrueRef = null;
        }

        Texture2D remoteTexture;

        public new Texture texture
        {
            get => remoteTexture;
        }

        /// <summary>
        /// Returns the texture used to draw this Graphic.
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                if (remoteTexture == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }

                    return s_WhiteTexture;
                }

                return remoteTexture;
            }
        }

#if UNITY_EDITOR


        protected override void OnValidate()
        {
            Debug.Log("RemoteRawImage.OnValidate()");
            base.OnValidate();

            Reload();
        }
#endif
    }
}