using UnityEditor;
using System.Collections;
using UnityEngine.UI;

[CustomEditor(typeof(UIFontSetter), false)]
public class UIFontSetterEditor : Editor
{
    SerializedProperty m_TargetTextProperty;

    protected virtual void OnEnable()
    {
        m_TargetTextProperty = serializedObject.FindProperty("textField");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var text = m_TargetTextProperty.objectReferenceValue as Text;

        if (text == null) m_TargetTextProperty.objectReferenceValue = (target as UIFontSetter).GetComponent<Text>();

        EditorGUILayout.PropertyField(m_TargetTextProperty);

        serializedObject.ApplyModifiedProperties();
    }
}