using System.Collections.Generic;
using Firebase.Analytics;
using UnityEngine;

public class Analytices : Network.Singleton<Analytices>
{
    // screenName, startTime
    private Dictionary<string, float> _screenDic = new();

    private const string DomainIdParamName = "domain_id";
    private const string DomainNameParamName = "domain_name";
    private const string UserIdParamName = "user_id";
    private const string DurationParamName = "duration";
    private const string SetupDomainParamName = "setup_domain_pressed";

    private void Awake()
    {
        DontDestroyOnLoad();
    }

    public void EnterScreen(string screenName)
    {
        if (_screenDic.ContainsKey(screenName))
        {
            _screenDic[screenName] = Time.time;
        }
        else
        {
            _screenDic.Add(screenName, Time.time);
        }
    }

    public void LeaveScreen(string screenName)
    {
#if UNITY_EDITOR
        return;
#endif
        if (_screenDic.ContainsKey(screenName))
        {
            float startTime = _screenDic[screenName];
            var parameterName = new Parameter(AnalyticsEnums.ParameterScreenName, screenName);
            var parameterTime = new Parameter(AnalyticsEnums.ParameterScreenTime, Time.time - startTime);
            FirebaseAnalytics.LogEvent(AnalyticsEnums.EventScreenView, parameterName, parameterTime);
            Logger.Log($"[{GetType().Name}] ScreenView: {screenName} {Time.time - startTime}");
            _screenDic.Remove(screenName);
        }
        else
        {
            Logger.LogError($"[{GetType().Name}] try to leave a wrong screen: {screenName}");
        }
    }

    public void LeaveScreen()
    {
        foreach (string screenName in _screenDic.Keys)
        {
            LeaveScreen(screenName);
        }
    }

    private void OnApplicationQuit()
    {
        LeaveScreen();
    }

    public void SinglePlayerDomainEvent(string appleUserId)
    {
#if UNITY_EDITOR
        return;
#endif
        var parameterUserId = new Parameter(UserIdParamName, appleUserId);
        FirebaseAnalytics.LogEvent(AnalyticsEnums.EventSinglePlayer, parameterUserId);
        Logger.Log($"[{GetType().Name}] Entered Single Player Mode: {appleUserId}");
    }

    public void MultiPlayerDomainEvent(string appleUserId)
    {
#if UNITY_EDITOR
        return;
#endif
        var parameterUserId = new Parameter(UserIdParamName, appleUserId);
        FirebaseAnalytics.LogEvent(AnalyticsEnums.EventMultiPlayer, parameterUserId);
        Logger.Log($"[{GetType().Name}] Entered Multi Player Mode: {appleUserId}");
    }

    public void CustomDomainEvent(string domainName, string domainId, string appleUserId)
    {
#if UNITY_EDITOR
        return;
#endif
        var parameterDomainId = new Parameter(DomainIdParamName, domainId);
        var parameterDomainName = new Parameter(DomainNameParamName, domainName);
        var parameterUserId = new Parameter(UserIdParamName, appleUserId);
        FirebaseAnalytics.LogEvent(AnalyticsEnums.EventCustomDomain, parameterDomainId, parameterDomainName, parameterUserId);
        Logger.Log($"[{GetType().Name}] Entered Custom Domain: {domainName}, user Id: {appleUserId}");
    }

    public void DomainSessionDuration(string domainName, string domainId, string appleUserId, double duration)
    {
#if UNITY_EDITOR
        return;
#endif
        var parameterDomainId = new Parameter(DomainIdParamName, domainId);
        var parameterDomainName = new Parameter(DomainNameParamName, domainName);
        var parameterUserId = new Parameter(UserIdParamName, appleUserId);
        var parameterDuration = new Parameter(DurationParamName, duration);
        FirebaseAnalytics.LogEvent(AnalyticsEnums.EventUserOnEachSessionDuration, parameterDomainId, parameterDomainName, parameterUserId, parameterDuration);
        Logger.Log($"[{GetType().Name}] Logged Duration Domain Name: {domainName}, user Id: {appleUserId}, duration: {duration}");
    }

    public void LogSetupDomainEvent()
    {
#if UNITY_EDITOR
        return;
#endif
        FirebaseAnalytics.LogEvent(AnalyticsEnums.EventSetupDomain);
    }
}