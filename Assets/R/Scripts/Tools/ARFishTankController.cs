using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARFishTankController : SceneInstance<ARFishTankController>
{
    public GameObject[] ARModeObjects;
    public GameObject[] TankModeObjects;

    protected bool mARMode = false;


    void Awake()
    {
#if UNITY_EDITOR

#endif
    }

    public void ToggleARCameraMode()
    {
        //TODO:
        //1. Request permission if no camera permission
        //2. Init Vuforia
        mARMode = !mARMode;
        SetARCameraActive(mARMode);
        EventManager.TriggerEvent(EventList.OnARModeActivated, mARMode);
    }

    public void SetARCameraActive(bool active)
    {
        RenderSettings.fog = !active;

        foreach (GameObject go in ARModeObjects)
        {
            go.SetActive(active);
        }

        foreach (GameObject go in TankModeObjects)
        {
            go.SetActive(!active);
        }
    }
}