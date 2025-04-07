using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions.Unity.ImageLoader
{
    [RequireComponent(typeof(AudioSource))]
    public class RemoteAudioSource : MonoBehaviour
    {
        public bool playOnLoad = false;
        public bool autoLoad = true;

        [SerializeField] private AudioSource m_AudioSource;

        [SerializeField] private string m_url = "";

        public string url
        {
            get => m_url;
            set
            {
                if (m_url != value)
                {
                    m_url = value;
                    dirty = true;
                    // Dirty = true;
                    if (autoLoad) Reload();
                }
            }
        }

        private bool dirty = true;


        IFuture<Reference<AudioClip>> future;
        Reference<AudioClip> clipRef;

        void OnLoaded(Reference<AudioClip> reference)
        {
            clipRef = reference;
            m_AudioSource.clip = clipRef.Value;
            if (playOnLoad)
            {
                m_AudioSource.Play();
            }
        }


        void OnFailed(Exception exception)
        {
            future?.Dispose();
            future = null;
        }

        public void Reload()
        {
            if (dirty || clipRef == null)
            {
                if (m_AudioSource.clip == null && clipRef is { IsDisposed: false })
                {
                    m_AudioSource.clip = clipRef.Value;
                }

                if (future != null && future.Url == m_url)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(url))
                {
                    m_AudioSource.clip = null;
                    clipRef?.Dispose();
                    clipRef = null;

                    future?.Dispose();
                    future = null;

                    dirty = false;

                    future = ImageLoader.LoadAudioRef(url)
                        .Loaded(OnLoaded)
                        .Failed(OnFailed)
                        .Canceled(() => { Debug.Log("RemoteAudioSource.Reload() cancelled"); });
                }
            }
        }

        private void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (autoLoad) Reload();
        }

        private void OnDestroy()
        {
            m_AudioSource.clip = null;
            future?.Dispose();
            clipRef?.Dispose();
        }
    }
}