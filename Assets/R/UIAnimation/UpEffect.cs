using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpEffect : MonoBehaviour
{
    public Vector2 Distance = new Vector2(0, 50);
    public float Speech = 1;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    protected Vector2 orig;
    protected Vector2 target;

    void OnEnable()
    {
        //Reset
        rectTransform = this.transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
        }

        orig = rectTransform.anchoredPosition;
        target = orig + Distance;
        count = 0;
    }

    float count = 0;

    // Update is called once per frame
    void Update()
    {
        count += Time.deltaTime * Speech;

        if (count <= 1)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(orig, target, count);
            canvasGroup.alpha = Mathf.Lerp(1, 0, count);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}