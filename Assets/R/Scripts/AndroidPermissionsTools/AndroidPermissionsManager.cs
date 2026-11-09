using System;
using UnityEngine;

public class AndroidPermissionCallback : AndroidJavaProxy
{
    private event Action<string> OnPermissionGrantedAction;
    private event Action<string> OnPermissionDeniedAction;

    public AndroidPermissionCallback(Action<string> onGrantedCallback, Action<string> onDeniedCallback)
        : base("com.unity3d.plugin.UnityAndroidPermissions$IPermissionRequestResult")
    {
        if (onGrantedCallback != null)
        {
            OnPermissionGrantedAction += onGrantedCallback;
        }

        if (onDeniedCallback != null)
        {
            OnPermissionDeniedAction += onDeniedCallback;
        }
    }

    // Handle permission granted
    public virtual void OnPermissionGranted(string permissionName)
    {
        Debug.Log("Permission " + permissionName + " GRANTED");
        if (OnPermissionGrantedAction != null)
        {
            OnPermissionGrantedAction(permissionName);
        }
    }

    // Handle permission denied
    public virtual void OnPermissionDenied(string permissionName)
    {
        Debug.Log("Permission " + permissionName + " DENIED!");
        if (OnPermissionDeniedAction != null)
        {
            OnPermissionDeniedAction(permissionName);
        }
    }
}

public class AndroidPermissionsManager
{
    private static AndroidJavaObject m_Activity;
    private static AndroidJavaObject m_PermissionService;

    private static AndroidJavaObject GetActivity()
    {
        if (m_Activity == null)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            m_Activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        return m_Activity;
    }

    private static AndroidJavaObject GetPermissionsService()
    {
        return m_PermissionService ??
               (m_PermissionService = new AndroidJavaObject("com.unity3d.plugin.UnityAndroidPermissions"));
    }

    public static bool IsPermissionGranted(string permissionName)
    {
#if UNITY_EDITOR
        return true;
#else
        return GetPermissionsService().Call<bool>("IsPermissionGranted", GetActivity(), permissionName);
#endif
    }

    public static void RequestPermission(string[] permissionNames, AndroidPermissionCallback callback)
    {
#if !UNITY_EDITOR
        GetPermissionsService().Call("RequestPermissionAsync", GetActivity(), permissionNames, callback);
#else
        foreach (string p in permissionNames)
        {
            callback.OnPermissionGranted(p);
        }
#endif
    }

    public static void RequestPermission(string[] permissionNames, Action<string> grantedCallback = null, Action<string> deniedCallback = null)
    {
        if (grantedCallback == null)
        {
            grantedCallback = GeneralGrantedCallback;
        }

        if (deniedCallback == null)
        {
            deniedCallback = GeneralDeniedCallback;
        }

#if !UNITY_EDITOR
        GetPermissionsService().Call("RequestPermissionAsync", GetActivity(), permissionNames, new AndroidPermissionCallback(grantedCallback, deniedCallback));
#else
        foreach (string p in permissionNames)
        {
            grantedCallback(p);
        }
#endif
    }

    protected static void GeneralGrantedCallback(string permission)
    {
        Debug.Log("Permission granted: " + permission);
    }

    protected static void GeneralDeniedCallback(string permission)
    {
        Debug.Log("Permission denied: " + permission);
    }
}