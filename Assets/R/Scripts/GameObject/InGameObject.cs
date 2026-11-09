using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameObject : MonoBehaviour
{
    protected InGameObjectData data;
    protected ObjectVisibleDetect visibleDetect;

    public InGameObjectData Data
    {
        get { return data; }
    }

    public bool IsVisible
    {
        get
        {
            if (visibleDetect != null)
            {
                return visibleDetect.IsVisible;
            }
            else
            {
                return false;
            }
        }
    }

    protected bool isActive = false;

    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }

    public void Init(InGameObjectData data)
    {
        this.data = data;

        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            visibleDetect = r.gameObject.AddComponent<ObjectVisibleDetect>();
        }

        isActive = true;
    }

    float timeCount = 0;

    void Update()
    {
        if (IsActive)
        {
            timeCount += Time.deltaTime;
            if (timeCount >= data.ExistTime && !IsVisible)
            {
                Destroy(this.gameObject);
            }
        }
    }
}