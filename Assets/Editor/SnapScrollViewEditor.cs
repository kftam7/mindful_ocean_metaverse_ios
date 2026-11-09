using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(SnapScrollView))]
public class SnapScrollViewEditor : ScrollRectEditor
{
    SerializedProperty snapDuration;
    SerializedProperty snapDurationByDistance;
    SerializedProperty onBeginDragCallback;
    SerializedProperty snappedCellCallback;

    protected override void OnEnable()
    {
        base.OnEnable();
        snapDuration = serializedObject.FindProperty("snapDuration");
        snapDurationByDistance = serializedObject.FindProperty("snapDurationByDistance");
        onBeginDragCallback = serializedObject.FindProperty("onBeginDragCallback");
        snappedCellCallback = serializedObject.FindProperty("snappedCellCallback");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        snapDuration.floatValue = EditorGUILayout.FloatField("Duration", snapDuration.floatValue);
        snapDurationByDistance.boolValue = EditorGUILayout.Toggle("By Distance", snapDurationByDistance.boolValue);
        EditorGUILayout.PropertyField(onBeginDragCallback, new GUIContent("Drag callback"));
        EditorGUILayout.PropertyField(snappedCellCallback, new GUIContent("Snap callback"));

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}