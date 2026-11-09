using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayoutSpace))]
public class LayoutSpaceEditor : Editor
{
    SerializedProperty ByRatio;
    SerializedProperty ReferenceByParent;
    SerializedProperty Target;
    SerializedProperty Ratio;
    SerializedProperty Pixel;

    void OnEnable()
    {
        ByRatio = serializedObject.FindProperty("ByRatio");
        ReferenceByParent = serializedObject.FindProperty("ReferenceByParent");
        Target = serializedObject.FindProperty("Target");
        Ratio = serializedObject.FindProperty("Ratio");
        Pixel = serializedObject.FindProperty("Pixel");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        ByRatio.boolValue = EditorGUILayout.Toggle("Use Ratio", ByRatio.boolValue);

        if (ByRatio.boolValue)
        {
            ReferenceByParent.boolValue = EditorGUILayout.Toggle("Local Ratio", ReferenceByParent.boolValue);
            if (!ReferenceByParent.boolValue)
            {
                Target.objectReferenceValue = EditorGUILayout.ObjectField("Ratio By", Target.objectReferenceValue, typeof(RectTransform), true);
            }

            Ratio.vector2Value = EditorGUILayout.Vector2Field("Ratio", Ratio.vector2Value);
        }
        else
        {
            Pixel.vector2Value = EditorGUILayout.Vector2Field("Pixel", Pixel.vector2Value);
        }

        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            (target as LayoutSpace).Start();
        }
    }
}