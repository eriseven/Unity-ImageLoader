using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Extensions.Unity.ImageLoader.UGUI
{
    [CustomEditor(typeof(RemoteImage))]
    public class RemoteImageEditor : ImageEditor
    {
        static Type baseType = typeof(ImageEditor);
        
        SerializedProperty m_url;
        
        GUIContent m_URLGUIContent;
        
        SerializedProperty m_PlaceHolder;
        
        GUIContent m_PlaceHolderContent;
        
        SerializedProperty m_Type;

        SerializedProperty m_Sprite;

        SerializedProperty m_PreserveAspect;

        SerializedProperty m_UseSpriteMesh;

        GUIContent m_SpriteContent;

        static FieldInfo showTypeInfo = baseType.GetField("m_ShowType", BindingFlags.NonPublic | BindingFlags.Instance);

        AnimBool m_ShowType
        {
            get => showTypeInfo?.GetValue(this) as AnimBool;

            set { showTypeInfo?.SetValue(this, value); }
        }
        


        static FieldInfo bIsDrivenInfo = baseType.GetField("m_bIsDriven", BindingFlags.NonPublic | BindingFlags.Instance);
        private bool m_bIsDriven
        {
            set => bIsDrivenInfo?.SetValue(this, value);
            get => bIsDrivenInfo != null && (bool)(bIsDrivenInfo.GetValue(this));
        }

        RemoteImage imageTarget => target as RemoteImage;
        protected override void OnEnable()
        {
            base.OnEnable();
            
            m_SpriteContent = EditorGUIUtility.TrTextContent("Source Image");
            
            m_Sprite                = serializedObject.FindProperty("m_Sprite");
            m_Type                  = serializedObject.FindProperty("m_Type");
            m_PreserveAspect        = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh         = serializedObject.FindProperty("m_UseSpriteMesh");

            m_bIsDriven = false;
            
            m_URLGUIContent = EditorGUIUtility.TrTextContent("Source URL");
            m_url = serializedObject.FindProperty("m_url");
            m_PlaceHolder = serializedObject.FindProperty("m_PlaceHolder");
            m_PlaceHolderContent = EditorGUIUtility.TrTextContent("Place Holder");
        }

        void UrlGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.DelayedTextField(m_url, m_URLGUIContent);
            if (EditorGUI.EndChangeCheck())
            {
                imageTarget.Dirty = true;
            }
            
            EditorGUILayout.PropertyField(m_PlaceHolder, m_PlaceHolderContent);
            if (GUILayout.Button("Refresh"))
            {
                imageTarget.Dirty = true;
            }
        }
        
        void SetShowNativeSize(bool instant)
        {
            Image.Type type = (Image.Type)m_Type.enumValueIndex;
            // bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) && m_Sprite.objectReferenceValue != null;
            
            Image image = target as Image;
            bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) && image.overrideSprite != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }
        
        new void SpriteGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(m_SpriteContent, imageTarget.overrideSprite, typeof(Sprite), false);
            EditorGUI.EndDisabledGroup();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            Image image = target as Image;
            RectTransform rect = image.GetComponent<RectTransform>();
            m_bIsDriven = (rect.drivenByObject as Slider)?.fillRect == rect;
            
            SpriteGUI();
            UrlGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            
            // m_ShowType.target = m_Sprite.objectReferenceValue != null;
            m_ShowType.target = image.overrideSprite != null;
            if (EditorGUILayout.BeginFadeGroup(m_ShowType.faded))
                TypeGUI();
            EditorGUILayout.EndFadeGroup();
            
            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;
            
                if ((Image.Type)m_Type.enumValueIndex == Image.Type.Simple)
                    EditorGUILayout.PropertyField(m_UseSpriteMesh);
            
                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}