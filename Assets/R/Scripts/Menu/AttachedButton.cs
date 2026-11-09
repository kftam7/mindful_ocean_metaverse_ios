using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachedButton : MonoBehaviour
{
    public Camera UsingCamera;
    public RectTransform UITarget;
    public Transform ContainerCanvas;

    void Update()
    {
        if (UITarget && UsingCamera && ContainerCanvas)
        {
            Vector3 screen = UsingCamera.WorldToScreenPoint(this.transform.position);
            UITarget.anchoredPosition3D = ContainerCanvas.InverseTransformPoint(screen);
        }
    }

    void OnEnable()
    {
        if (UITarget)
        {
            UITarget.gameObject.SetActive(true);
        }
    }

    void OnDisable()
    {
        if (UITarget)
        {
            UITarget.gameObject.SetActive(false);
        }
    }
}