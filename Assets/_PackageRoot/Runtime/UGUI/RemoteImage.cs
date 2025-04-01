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
        Reference<Texture2D>  textrueRef;

        private bool dirty = true;

        void Reload()
        {
            if (dirty || overrideSprite == null)
            {
                if (future != null && future.Url == m_url)
                {
                    return;
                }
                
                Debug.Log("RemoteImage.Reload()");
                if (!string.IsNullOrEmpty(url))
                {
                    this.overrideSprite = null;
                    textrueRef?.Dispose();
                    textrueRef = null;
                    
                    future?.Dispose();
                    future = null;
                    dirty = false;
                    future = ImageLoader.LoadTextureRef(url)
                        .Loaded(reference =>
                        {
                            Debug.Log("RemoteImage.Reload() Loaded");
                            reference.DisposeOnDisable(this);
                            this.overrideSprite = reference.Value.ToSprite();
                            textrueRef = reference;
                        })
                        .Failed(reference =>
                        {
                            Debug.Log("RemoteImage.Reload() failed");
                            future?.Dispose();
                            future = null;
                        })
                        .CancelOnDisable(this)
                        .Canceled(() => {Debug.Log("RemoteImage.Reload() cancelled");});
                        // .Consume(this);
                    future.Forget();
                }
            }
        }

        protected override void OnEnable()
        {
            Debug.Log("RemoteImage.OnEnable()");
            base.OnEnable();
            Reload();
        }

        protected override void OnDisable()
        {
            Debug.Log("RemoteImage.OnDisable()");
            
            this.overrideSprite = null;
            base.OnDisable();
            future = null;
            
            textrueRef?.Dispose();
            textrueRef = null;
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