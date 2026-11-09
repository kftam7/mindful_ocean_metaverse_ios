using UnityEngine;
using UnityEditor;

public class CopyMaterialData : EditorWindow
{
    [MenuItem("Tools/CopyPropertiesFromMaterial")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        CopyMaterialData window = (CopyMaterialData)EditorWindow.GetWindow(typeof(CopyMaterialData));
        window.Show();
    }

    public Object source;
    public bool withoutTexture;
    public string[] Field = { "_MainTex" };

    void OnGUI()
    {
        source = EditorGUILayout.ObjectField(source, typeof(Material), true);

        EditorGUILayout.Space();

        withoutTexture = EditorGUILayout.Toggle("Without texture", withoutTexture);

        if (withoutTexture)
        {
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty stringsProperty = so.FindProperty("Field");

            EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
            so.ApplyModifiedProperties(); // Remember to apply modified properties
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Properties"))
        {
            Apply();
        }
    }

    protected void Apply()
    {
        Material model = source as Material;

        if (model == null) return;

        object[] selection = Selection.objects;

        for (int i = 0; i < selection.Length; i++)
        {
            Material selectedObject = selection[i] as Material;
            if (selectedObject != null && selectedObject != model)
            {
                Texture[] textures = null;

                if (withoutTexture && Field.Length > 0)
                {
                    textures = new Texture[Field.Length];

                    for (int m = 0; m < Field.Length; m++)
                    {
                        textures[m] = selectedObject.GetTexture(Field[m]);
                    }
                }

                selectedObject.CopyPropertiesFromMaterial(model);


                if (withoutTexture && Field.Length > 0)
                {
                    for (int n = 0; n < Field.Length; n++)
                    {
                        selectedObject.SetTexture(Field[n], textures[n]);
                    }
                }


                Debug.Log(string.Format("Copyed from {0} to {1}", model.name, selectedObject.name));

                EditorUtility.SetDirty(selectedObject);
            }
        }

        AssetDatabase.SaveAssets();
    }
}