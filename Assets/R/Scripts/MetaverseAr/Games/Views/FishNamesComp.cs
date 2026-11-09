using UnityEngine;
using UnityEngine.UI;

public class FishNamesComp : MonoBehaviour
{
    [SerializeField] private Text textComponent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    public void SetName(string name)
    {
        textComponent.text = name;
    }

    public void SetVisibility(bool visible)
    {
        canvasGroup.alpha = visible ? 1 : 0;
    }

    public void SetAnchoredPosition(Vector2 pos)
    {
        rectTransform.anchoredPosition = pos;
    }
}