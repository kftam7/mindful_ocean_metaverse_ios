using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ListenEvent : UnityEvent<object> { }

public class EventManager : MonoBehaviour
{
    protected Dictionary<string, ListenEvent> eventDictionary;

    protected static EventManager eventManager;
    private static System.Object thisLock = new System.Object();


    public static EventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                if (!eventManager)
                {
                    lock (thisLock)
                    {
                        GameObject go = new GameObject("EventManage");
                        eventManager = go.AddComponent<EventManager>();
                    }
                }

                eventManager.Init();
            }

            return eventManager;
        }
    }

    protected virtual void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, ListenEvent>();
        }
    }

    public static void StartListening(string eventName, UnityAction<object> listener)
    {
        ListenEvent thisEvent = null;

        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new ListenEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction<object> listener)
    {
        if (eventManager == null)
            return;

        ListenEvent thisEvent = null;

        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName, object message)
    {
        ListenEvent thisEvent = null;

        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(message);
        }
    }

    public static void TriggerEvent(string eventName)
    {
        TriggerEvent(eventName, null);
    }
}