using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader.UGUI
{
    [CustomEditor(typeof(RemoteRawImage))]
    public class RemoteRawImageEditor : RawImageEditor
    {
        SerializedProperty m_url;

        GUIContent m_URLGUIContent;

        SerializedProperty m_Texture;
        SerializedProperty m_UVRect;
        GUIContent m_UVRectContent;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Note we have precedence for calling rectangle for just rect, even in the Inspector.
            // For example in the Camera component's Viewport Rect.
            // Hence sticking with Rect here to be consistent with corresponding property in the API.
            m_UVRectContent = EditorGUIUtility.TrTextContent("UV Rect");

            m_Texture = serializedObject.FindProperty("m_Texture");
            m_UVRect = serializedObject.FindProperty("m_UVRect");

            SetShowNativeSize(true);

            m_URLGUIContent = EditorGUIUtility.TrTextContent("Source URL");
            m_url = serializedObject.FindProperty("m_url");
        }

        RemoteRawImage rawImageTarget => target as RemoteRawImage;

        void UrlGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.DelayedTextField(m_url, m_URLGUIContent);
            if (EditorGUI.EndChangeCheck())
            {
                rawImageTarget.Dirty = true;
            }

            if (GUILayout.Button("Refresh"))
            {
                rawImageTarget.Dirty = true;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.ObjectField(rawImageTarget.mainTexture, typeof(Texture), false);
            // EditorGUILayout.PropertyField(m_Texture);
            EditorGUI.EndDisabledGroup();
            UrlGUI();

            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            EditorGUILayout.PropertyField(m_UVRect, m_UVRectContent);
            SetShowNativeSize(false);
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            base.SetShowNativeSize(rawImageTarget.texture != null, instant);
        }
    }
}