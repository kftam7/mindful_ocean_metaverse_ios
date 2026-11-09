using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIIPhoneXObj : MonoBehaviour
{
    public bool isLandscapeRight, isLandscapeLeft;

    // Use this for initialization
    void Start()
    {
        if (!SystemExtension.isIPhoneXScreen())
        {
            gameObject.SetActive(false);
            return;
        }

        OnOrientationChange(Input.deviceOrientation);
        DeviceChange.OnOrientationChange += OnOrientationChange;
    }

    //private void Update()
    //{
    //    Debug.Log("OnRectTransformDimensionsChange: "+Input.deviceOrientation);
    //}

    private void OnOrientationChange(DeviceOrientation orientation)
    {
        Debug.Log("OnOrientationChange: " + orientation);
        switch (orientation)
        {
            case DeviceOrientation.LandscapeLeft:
                gameObject.SetActive(false);
                if (isLandscapeRight) gameObject.SetActive(true);
                break;
            case DeviceOrientation.LandscapeRight:
                gameObject.SetActive(false);
                if (isLandscapeLeft) gameObject.SetActive(true);
                break;
        }
    }
}