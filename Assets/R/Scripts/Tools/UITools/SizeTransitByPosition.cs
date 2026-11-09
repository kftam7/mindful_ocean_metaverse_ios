using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeTransitByPosition : MonoBehaviour
{
    public Transform ParentCanvas;
    public Vector3 MaxSize = Vector3.one;
    public Vector3 MinSize = new Vector3(0.7f, 0.7f, 0.7f);
    public Vector2 FocusArea = Vector2.zero;

    public Vector3 ScreenPosition = Vector3.zero;

    public void UpdatePosition()
    {
        ScreenPosition = ParentCanvas.InverseTransformPoint(this.transform.position);

        UpdateSize(Mathf.Clamp01((FocusArea.x - Mathf.Abs(ScreenPosition.x)) / FocusArea.x));
    }

    public void UpdateSize(float progress)
    {
        this.transform.localScale = Vector3.Lerp(MinSize, MaxSize, progress);
    }

    void Update()
    {
        UpdatePosition();
    }
}