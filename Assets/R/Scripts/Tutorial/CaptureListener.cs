using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureListener : GeneralTutorialProgress
{
    void OnEnable()
    {
        EventManager.StartListening(EventList.OnTrackingReturnInBounds, ReturnInBounds);
    }

    void OnDisable()
    {
        EventManager.StopListening(EventList.OnTrackingReturnInBounds, ReturnInBounds);
    }


    protected void ReturnInBounds(object results)
    {
        OnTutorialProgressEnd();
    }
}