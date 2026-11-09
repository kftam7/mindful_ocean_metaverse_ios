using System.Diagnostics;
using UnityEngine;

public static class Logger
{
    #region DEBUG.Log Wrapper
    /// <summary>
    /// Debug.Log wrapper with required scripting defined symbols "LOG_ENABLED"
    /// </summary>
    /// <param name="format">Log message</param>
    /// <param name="obj">obj params</param>
    [Conditional("LOG_ENABLED")]
    public static void Log(string format, params object[] obj)
    {
        UnityEngine.Debug.LogFormat(format, obj);
    }
    
    [Conditional("LOG_ENABLED")]
    public static void Log(string format)
    {
        UnityEngine.Debug.Log(format);
    }

    /// <summary>
    /// Debug.Log wrapper with required scripting defined symbols "LOG_ENABLED"
    /// </summary>
    /// <param name="context">Gameobject context</param>
    /// <param name="format">Log message</param>
    /// <param name="obj">obj params</param>
    [Conditional("LOG_ENABLED")]
    public static void Log(Object context, string format, params object[] obj)
    {
        UnityEngine.Debug.LogFormat(context, format, obj);
    }

    /// <summary>
    /// Debug.LogError wrapper with required scripting defined symbols "LOG_ENABLED"
    /// </summary>
    /// <param name="format">Log message</param>
    /// <param name="obj">obj params</param>
    [Conditional("LOG_ENABLED")]
    public static void LogWarning(string format, params object[] obj)
    {
        UnityEngine.Debug.LogWarningFormat(format, obj);
    }
    
    [Conditional("LOG_ENABLED")]
    public static void LogWarning(string format)
    {
        UnityEngine.Debug.LogWarning(format);
    }

    /// <summary>
    /// Debug.LogError wrapper with required scripting defined symbols "LOG_ENABLED"
    /// </summary>
    /// <param name="context">Gameobject context</param>
    /// <param name="format">Log message</param>
    /// <param name="obj">obj params</param>
    [Conditional("LOG_ENABLED")]
    public static void LogWarning(Object context, string format, params object[] obj)
    {
        UnityEngine.Debug.LogWarningFormat(context, format, obj);
    }

    /// <summary>
    /// Debug.LogError wrapper with required scripting defined symbols "LOG_ENABLED"
    /// </summary>
    /// <param name="format">Log message</param>
    /// <param name="obj">obj params</param>
    [Conditional("LOG_ENABLED")]
    public static void LogError(string format, params object[] obj)
    {
        UnityEngine.Debug.LogErrorFormat(format, obj);
    }
    
    [Conditional("LOG_ENABLED")]
    public static void LogError(string format)
    {
        UnityEngine.Debug.LogError(format);
    }

    /// <summary>
    /// Debug.LogError wrapper with required scripting defined symbols "LOG_ENABLED"
    /// </summary>
    /// <param name="context">Gameobject context</param>
    /// <param name="format">Log message</param>
    /// <param name="obj">obj params</param>
    [Conditional("LOG_ENABLED")]
    public static void LogError(Object context, string format, params object[] obj)
    {
        UnityEngine.Debug.LogErrorFormat(context, format, obj);
    }
    #endregion
}