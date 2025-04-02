using System;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

        [SerializeField] private Texture2D m_PlaceHolder;

        private Sprite m_PlaceHolderSprite;

        private Sprite PlaceHolderSprite
        {
            get
            {
                if (m_PlaceHolderSprite.IsNull() && m_PlaceHolder.IsNotNull())
                {
                    m_PlaceHolderSprite = m_PlaceHolder.ToSprite();
                }

                return m_PlaceHolderSprite;
            }
        }

        private bool dirty = true;

        void OnLoaded(Reference<Texture2D> reference)
        {
            Debug.Log("RemoteImage.Reload() Loaded");
            textrueRef = reference;
            overrideSprite = reference.Value.ToSprite();
        }

        void OnFailed(Exception exception)
        {
            Debug.Log("RemoteImage.Reload() failed");
            if (overrideSprite == PlaceHolderSprite)
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
                    overrideSprite = PlaceHolderSprite;
                    textrueRef?.Dispose();
                    textrueRef = null;

                    future?.Dispose();
                    future = null;
                    dirty = false;

                    future = ImageLoader.LoadTextureRef(url)
                        .Loaded(OnLoaded)
                        .Failed(OnFailed)
                        // .Consume(this)
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

        [MenuItem("CONTEXT/Image/Convert to RemoteImage")]
        public static void ReplaceImageWithRemoteImage(MenuCommand command)
        {
            if (command.context == null) return;

            var monoBehaviour = command.context as MonoBehaviour;

            var guids = AssetDatabase.FindAssets("RemoteImage t:MonoScript glob:\"Runtime/UGUI/**\"");
            if (guids == null || guids.Length == 0)
            {
                return;
            }
            var chosenTextAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(UnityEngine.Object));
            
            if (chosenTextAsset == null)
            {
                return;
            }

            Undo.RegisterCompleteObjectUndo(command.context, "Changing component script");

            var so = new SerializedObject(monoBehaviour);
            var scriptProperty = so.FindProperty("m_Script");
            var spriteProperty = so.FindProperty("m_Sprite");
            so.Update();
            scriptProperty.objectReferenceValue = chosenTextAsset;
            spriteProperty.objectReferenceValue = null;
            so.ApplyModifiedProperties();
        }
#endif
    }
}