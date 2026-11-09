using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubeFishTank))]
public class FishTankEditor : Editor
{
    float tankSize = 1;

    void OnEnable()
    {
        Transform tf = (target as FishTank).transform.parent;
        if (tf != null)
        {
            tankSize = tf.localScale.x;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        tankSize = EditorGUILayout.Slider(tankSize, 0.1f, 2.5f);

        if (EditorGUI.EndChangeCheck())
        {
            Transform tf = (target as FishTank).transform.parent;
            if (tf != null)
            {
                tf.localScale = Vector3.one * tankSize;
                (target as FishTank).SetTankSize(tankSize);
            }
            else
            {
                Debug.Log("Tank should be under container");
            }
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Delete all"))
        {
            (target as FishTank).DestroyAllFishes();
        }

        if (GUILayout.Button("Re-generate"))
        {
            (target as FishTank).InitFishTank();
            (target as FishTank).CreateFishes(ImageDataManager.Instance.ImageDataList, 0, Vector3.zero);
        }

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(CylinderFishTank))]
public class CylinderFishTankEditor : FishTankEditor { }

[CustomEditor(typeof(TubeFishTank))]
public class TubeFishTankEditor : FishTankEditor { }