using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace iPhoneXTools
{
    [CustomEditor(typeof(iPhoneXFix))]
    public class iPhoneXFixEditor : Editor
    {

        [MenuItem("Tool/Enable iPhoneX Zone")]
        static void ActivateIPhoneXSafeZone()
        {
            EditorPrefs.SetBool("IPHONEX_SAFE_ZONE", true);
        }

        [MenuItem("Tool/Disable iPhoneX Zone")]
        static void DeactivateIPhoneXSafeZone()
        {
            EditorPrefs.SetBool("IPHONEX_SAFE_ZONE", false);
        }

        SerializedProperty autoSafeZone;
        SerializedProperty safeZone;
        SerializedProperty testInEditor;

        void OnEnable()
        {
            autoSafeZone = serializedObject.FindProperty("AutoSafeZone");
            safeZone = serializedObject.FindProperty("SafeZone");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            autoSafeZone.boolValue = EditorGUILayout.Toggle("Auto Safe Zone ", autoSafeZone.boolValue);

            if (!autoSafeZone.boolValue)
            {

                safeZone.rectValue = EditorGUILayout.RectField("Safe Zone ", safeZone.rectValue);
            }

            serializedObject.ApplyModifiedProperties();

            // if(EditorGUI.EndChangeCheck() && EditorApplication.isPlaying){

            //     (target as iPhoneXFix).AdjustSafeAreaByEditor();
            // }
        }
    }
}