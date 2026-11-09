using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;
using UnityEngine.Analytics;
// using KHD;
using System.Linq;

public class AnalyticsLogCenter : ProGamer.Singleton<AnalyticsLogCenter>
{
    public static void LogEvent(string eventName, IDictionary<string, object> eventData = null)
    {
        // AnalyticsEvent.Custom(eventName, eventData);
        if (eventData != null)
        {
            // FlurryAnalytics.Instance.LogEventWithParameters(eventName, eventData.ToDictionary(k => k.Key, k => k.Value.ToString()));
        }
        else
        {
            // FlurryAnalytics.Instance.LogEvent(eventName);
        }

#if UNITY_EDITOR
        Debug.Log("Log event: " + eventName);
#endif
    }
}