using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARDetection : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.StartListening(EventList.OnTrackingFound, OnTrackingFound);
        EventManager.StartListening(EventList.OnTrackingLost, OnTrackingLost);
        EventManager.StartListening(EventList.OnARModeActivated, OnARModeActivated);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnTrackingFound, OnTrackingFound);
        EventManager.StopListening(EventList.OnTrackingLost, OnTrackingLost);
        EventManager.StopListening(EventList.OnARModeActivated, OnARModeActivated);
    }

    public void OnARModeActivated(object result)
    {
        bool active = (bool)result;
        SetGameObjectsVisibility(!active);
    }

    public void OnTrackingFound(object result)
    {
        SetGameObjectsVisibility(true);
    }

    public void OnTrackingLost(object result)
    {
        SetGameObjectsVisibility(false);
    }

    public void SetGameObjectsVisibility(bool isVisiable)
    {
        Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
        Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

        // Enable rendering:
        foreach (Renderer component in rendererComponents)
        {
            component.enabled = isVisiable;
        }

        // Enable colliders:
        foreach (Collider component in colliderComponents)
        {
            component.enabled = isVisiable;
        }
    }
}