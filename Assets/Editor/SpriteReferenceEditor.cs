using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Newtonsoft.Json;

[CustomEditor(typeof(SpriteReference))]
[CanEditMultipleObjects]
public class SpriteReferenceEditor : Editor
{
    [MenuItem("GameObject/Create SpriteReference", false, 1)]
    static void CreateSpriteReference()
    {
        GameObject go = new GameObject("SpriteReference");
        go.AddComponent<SpriteReference>();
    }

    SerializedProperty spriteArray;

    void OnEnable()
    {
        spriteArray = serializedObject.FindProperty("spriteList");
    }

    string listString = "";
    bool inputOption = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(spriteArray, true);

        EditorGUILayout.Space();

        if (GUILayout.Button("Clear empty entry"))
        {
            ClearEmptyArrayEntry();
            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Rearrange"))
        {
            RearrangeArray();
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        inputOption = GUILayout.Toggle(inputOption, "Input/Output reference");
        if (inputOption)
        {
            listString = GUILayout.TextArea(listString);

            if (GUILayout.Button("Output list"))
            {
                listString = GetSpriteNameList();
            }

            if (GUILayout.Button("Input list"))
            {
                if (!string.IsNullOrEmpty(listString))
                {
                    AddSpriteToList(SearchInputData(listString));
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void ClearEmptyArrayEntry()
    {
        SpriteReference sr = (SpriteReference)target;
        sr.ClearEmptyArrayEntry();
    }

    public void RearrangeArray()
    {
        SpriteReference sr = (SpriteReference)target;
        sr.ClearEmptyArrayEntry(true);
    }

    public string GetSpriteNameList()
    {
        SpriteReference sr = (SpriteReference)target;
        return sr.GetSpriteList();
    }

    public void AddSpriteToList(List<Sprite> list)
    {
        SpriteReference sr = (SpriteReference)target;

        if (list != null && list.Count > 0)
        {
            foreach (Sprite s in list)
            {
                sr.AddSprite(s);
            }
        }
    }

    public List<Sprite> SearchInputData(string jsonList)
    {
        List<string> spriteList = JsonConvert.DeserializeObject<List<string>>(jsonList);
        List<Sprite> resultSprite = new List<Sprite>();

        if (spriteList != null && spriteList.Count > 0)
        {
            Debug.Log("Please ensure the sprite name is unqiue");
            foreach (string name in spriteList)
            {
                string[] results = AssetDatabase.FindAssets(string.Format("{0} t:Sprite", name));

                if (results != null && results.Length > 0)
                {
                    foreach (string guid in results)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (s.name == name)
                        {
                            resultSprite.Add(s);
                            Debug.Log(string.Format("Added {0} from {1}", name, path));
                        }
                    }
                }
                else
                {
                    Debug.LogWarning(string.Format("Cannot find sprite {0}", name));
                }
            }
        }

        return resultSprite;
    }
}