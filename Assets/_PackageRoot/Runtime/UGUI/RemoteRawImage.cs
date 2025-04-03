using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

        [SerializeField] private Texture2D m_PlaceHolder;


        void OnLoaded(Reference<Texture2D> reference)
        {
            Debug.Log("RemoteRawImage.Reload() Loaded");
            // remoteTexture = reference.Value;
            textrueRef = reference;
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
                        .Consume(this)
                        // .SetPlaceholder(placeHoder, Color.white, PlaceholderTrigger.LoadingFromSource);
                        .Canceled(() => { Debug.Log("RemoteRawImage.Reload() cancelled"); });

                    future.Forget();
                }
                else
                {
                    remoteTexture = null;
                }
            }
        }

        protected override void OnEnable()
        {
            Debug.Log("RemoteRawImage.OnEnable()");
            base.OnEnable();
            Reload();
        }

        protected override void OnDestroy()
        {
            Debug.Log("RemoteRawImage.OnDestroy()");
            base.OnDisable();
            future?.Dispose();
            textrueRef?.Dispose();
        }

        // protected override void OnDisable()
        // {
        //     Debug.Log("RemoteRawImage.OnDisable()");
        //
        //     remoteTexture = null;
        //     base.OnDisable();
        //     future = null;
        //
        //     textrueRef?.Dispose();
        //     textrueRef = null;
        // }

        private Texture2D m_RemoteTexture;

        public Texture2D remoteTexture
        {
            get => m_RemoteTexture;
            set
            {
                if (m_RemoteTexture != value)
                {
                    m_RemoteTexture = value;
                    SetVerticesDirty();
                    SetMaterialDirty();
                }
            }
        }

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
                    if (textrueRef is { IsDisposed: false })
                    {
                        m_RemoteTexture = textrueRef.Value;
                        return remoteTexture;
                    }

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

        [MenuItem("CONTEXT/RawImage/Convert to RemoteRawImage")]
        public static void ReplaceImageWithRemoteImage(MenuCommand command)
        {
            if (command.context == null) return;

            var monoBehaviour = command.context as MonoBehaviour;

            var guids = AssetDatabase.FindAssets("RemoteRawImage t:MonoScript glob:\"Runtime/UGUI/**\"");
            if (guids == null || guids.Length == 0)
            {
                return;
            }

            var chosenTextAsset =
                AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(UnityEngine.Object));

            if (chosenTextAsset == null)
            {
                return;
            }

            Undo.RegisterCompleteObjectUndo(command.context, "Changing component script");

            var so = new SerializedObject(monoBehaviour);
            var scriptProperty = so.FindProperty("m_Script");
            var textureProperty = so.FindProperty("m_Texture");
            so.Update();
            scriptProperty.objectReferenceValue = chosenTextAsset;
            textureProperty.objectReferenceValue = null;
            so.ApplyModifiedProperties();
        }
#endif
    }
}