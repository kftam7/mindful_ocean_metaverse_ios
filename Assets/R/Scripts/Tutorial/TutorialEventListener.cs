using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventListener : GeneralTutorialProgress
{
    public string EventName;

    void OnEnable()
    {
        if (!string.IsNullOrEmpty(EventName))
        {
            EventManager.StartListening(EventName, Trigger);
        }
    }

    void OnDisable()
    {
        if (!string.IsNullOrEmpty(EventName))
        {
            EventManager.StopListening(EventName, Trigger);
        }
    }

    protected void Trigger(object results)
    {
        OnTutorialProgressEnd();
    }

    protected void Skip()
    {
        OnTutorialProgressSkippedCallback();
    }
}