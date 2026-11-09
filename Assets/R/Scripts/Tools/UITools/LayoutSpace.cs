using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutSpace : MonoBehaviour
{
    public bool ByRatio = false;
    public bool ReferenceByParent = true;
    public RectTransform Target;
    public Vector2 Ratio = new Vector2(0.1f, 0.1f);
    public Vector2 Pixel = new Vector2(100f, 100f);

    public void Start()
    {
        if (ByRatio)
        {
            RectTransform t = null;
            if (ReferenceByParent)
            {
                t = this.transform.parent as RectTransform;
            }
            else
            {
                t = Target;
            }

            if (t == null)
            {
                if (ReferenceByParent)
                {
                    Debug.LogError("The parent is null or isn't RectTransform");
                }
                else
                {
                    Debug.LogError("Please assign a target");
                }

                return;
            }

            Vector3[] corner = new Vector3[4];

            t.GetLocalCorners(corner);

            Pixel = new Vector2((corner[3].x - corner[0].x) * Ratio.x, (corner[1].y - corner[0].y) * Ratio.y);
        }

        var rectTransform = this.transform as RectTransform;
        if (rectTransform)
        {
            Debug.Log("Set sizeDelta: " + Pixel.ToString());
            rectTransform.sizeDelta = Pixel;
        }
    }
}