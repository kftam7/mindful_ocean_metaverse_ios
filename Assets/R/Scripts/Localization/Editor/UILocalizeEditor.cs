using UnityEditor;
using System.Collections;
using UnityEngine.UI;

namespace ProGamer
{
    [CustomEditor(typeof(UILocalize), false)]
    public class UILocalizeEditor : Editor
    {
        SerializedProperty m_TargetTextProperty;
        SerializedProperty m_TargetKeyProperty;

        protected virtual void OnEnable()
        {
            m_TargetTextProperty = serializedObject.FindProperty("textField");
            m_TargetKeyProperty = serializedObject.FindProperty("key");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var text = m_TargetTextProperty.objectReferenceValue as Text;

            if (text == null)
                m_TargetTextProperty.objectReferenceValue = (target as UILocalize).GetComponent<Text>();

            EditorGUILayout.PropertyField(m_TargetTextProperty);
            EditorGUILayout.PropertyField(m_TargetKeyProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}