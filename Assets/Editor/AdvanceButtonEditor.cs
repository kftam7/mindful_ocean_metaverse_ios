using UnityEngine;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(AdvanceButton), true)]
    [CanEditMultipleObjects]
    public class AdvanceButtonEditor : ButtonEditor
    {
        bool showAdvanceSetting = false;

        SerializedProperty m_OnClickSound;

        SerializedProperty m_OnPointerClick;
        SerializedProperty m_OnPointerDown;
        SerializedProperty m_OnPointerUp;
        SerializedProperty m_OnPointerEnter;
        SerializedProperty m_OnPointerExit;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickSound = serializedObject.FindProperty("onClickSound");
            m_OnPointerClick = serializedObject.FindProperty("onPointerClickEvent");
            m_OnPointerDown = serializedObject.FindProperty("onPointerDownEvent");
            m_OnPointerUp = serializedObject.FindProperty("onPointerUpEvent");
            m_OnPointerEnter = serializedObject.FindProperty("onPointerEnterEvent");
            m_OnPointerExit = serializedObject.FindProperty("onPointerExitEvent");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();

            showAdvanceSetting = EditorGUILayout.Foldout(showAdvanceSetting, "Advance Setting");

            if (showAdvanceSetting)
            {
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(m_OnClickSound);
                EditorGUILayout.PropertyField(m_OnPointerClick);
                EditorGUILayout.PropertyField(m_OnPointerDown);
                EditorGUILayout.PropertyField(m_OnPointerUp);
                EditorGUILayout.PropertyField(m_OnPointerEnter);
                EditorGUILayout.PropertyField(m_OnPointerExit);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}