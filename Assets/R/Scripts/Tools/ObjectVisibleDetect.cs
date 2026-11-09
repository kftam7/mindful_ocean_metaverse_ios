using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectVisibleDetect : MonoBehaviour
{
    protected bool isVisible = false;

    public bool IsVisible
    {
        get { return isVisible; }
    }

    void OnBecameVisible()
    {
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;
    }
}