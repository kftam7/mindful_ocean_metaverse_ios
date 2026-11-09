using UnityEngine;
using UnityEditor;

public class LayerSetter : EditorWindow
{
    string[] layersName;
    SortingLayer[] layers;
    bool isChild = true;
    int selected = 0;

    bool isSetLayer = false;

    int layerIndex = 0;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Layer Setter")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        LayerSetter window = (LayerSetter)EditorWindow.GetWindow(typeof(LayerSetter));
        window.Show();
    }

    void OnFocus()
    {
        RefreshLayer();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        selected = EditorGUILayout.Popup("Sorting Layer", selected, layersName);
        if (GUILayout.Button("Refresh"))
        {
            RefreshLayer();
        }

        EditorGUILayout.Space();
        isChild = EditorGUILayout.Toggle("Include Childs", isChild);

        isSetLayer = EditorGUILayout.Toggle("Set Layer Order", isSetLayer);

        layerIndex = EditorGUILayout.IntField("Order in Layer", layerIndex);

        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Layer"))
        {
            ApplyLayer();
        }
    }

    void ApplyLayer()
    {
        object[] selection = Selection.objects;

        GameObject selectedObject;

        for (int i = 0; i < selection.Length; i++)
        {
            selectedObject = selection[i] as GameObject;
            if (selectedObject != null)
            {
                Renderer r = selectedObject.GetComponent<Renderer>();
                if (r != null)
                {
                    r.sortingLayerName = layersName[selected];
                    if (isSetLayer) r.sortingOrder = layerIndex;
                }

                if (isChild)
                {
                    Renderer[] rs = selectedObject.GetComponentsInChildren<Renderer>(true);
                    if (rs != null)
                    {
                        for (int t = 0; t < rs.Length; t++)
                        {
                            rs[t].sortingLayerName = layersName[selected];
                            if (isSetLayer) rs[t].sortingOrder = layerIndex;
                        }
                    }
                }
            }
        }
    }

    void RefreshLayer()
    {
        layers = SortingLayer.layers;
        layersName = new string[layers.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            layersName[i] = layers[i].name;
        }
    }
}