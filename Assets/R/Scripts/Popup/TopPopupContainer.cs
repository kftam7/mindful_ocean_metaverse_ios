using UnityEngine;

[DisallowMultipleComponent]
public class TopPopupContainer : MonoBehaviour
{
    void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
    }

    protected static TopPopupContainer instance;

    public static TopPopupContainer Instance
    {
        get
        {
            if (!instance)
            {
                GameObject go = new GameObject("TopPopupContainer");

                go.transform.SetParent(TopCanvas.Instance.CanvasContainer);

                go.AddComponent<RectTransform>();
                instance = go.AddComponent<TopPopupContainer>();

                RectTransform rect = go.transform as RectTransform;

                if (rect != null)
                {
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                    rect.anchoredPosition3D = Vector3.zero;

                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;

                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.one;

                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.SetAsLastSibling();
                }
            }

            return instance;
        }
    }
}