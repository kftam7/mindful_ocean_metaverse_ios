using UnityEngine;
using UnityEngine.UI;

public static class UiUtils
{
    public static void RefreshLayoutGroupsImmediateAndRecursive(this RectTransform root)
    {
        var componentsInChildren = root.GetComponentsInChildren<LayoutGroup>(true);
        foreach (var layoutGroup in componentsInChildren)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }
}