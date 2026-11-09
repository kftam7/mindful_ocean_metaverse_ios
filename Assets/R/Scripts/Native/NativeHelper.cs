using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;

public enum PermissionStatus
{
    Undetermined,
    Granted,
    Denied
}

public class NativeHelper : ProGamer.Singleton<NativeHelper>
{
    public static Action<PermissionStatus> onCameraPermissionUpdated = delegate { };

    public static void OpenSetting()
    {
#if UNITY_EDITOR
        Debug.Log("OpenSetting");
#elif UNITY_IOS
        NativeHelperIOS.OpenSetting();
#else
		try
        {
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                string packageName = currentActivityObject.Call<string>("getPackageName");

                using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                {
                    intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                    intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                    currentActivityObject.Call("startActivity", intentObject);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
#endif
    }

    public static PermissionStatus GetCameraPermission()
    {
#if UNITY_EDITOR
        return PermissionStatus.Granted;
#elif UNITY_IOS
        return ParseEnum<PermissionStatus>(NativeHelperIOS.GetCameraPermission());
#else
        if(PlayerPrefs.GetInt("REQUIRED_CAMERA", 0) == 1){

            return AndroidPermissionsManager.IsPermissionGranted("android.permission.CAMERA") ? PermissionStatus.Granted : PermissionStatus.Denied;

        }else{

            return PermissionStatus.Undetermined;
        }
#endif
    }

    public static void RequestCameraPermission()
    {
#if UNITY_EDITOR
        Debug.Log("RequestCameraPermission");
        onCameraPermissionUpdated(PermissionStatus.Granted);
#elif UNITY_IOS
        NativeHelperIOS.RequestCameraPermission();
#else
        PlayerPrefs.SetInt("REQUIRED_CAMERA", 1);
        AndroidPermissionsManager.RequestPermission(new string[] { "android.permission.CAMERA" },
                                                    (string permission) =>
                                                    {
                                                        onCameraPermissionUpdated(PermissionStatus.Granted);
                                                    },
                                                    (string permission) =>
                                                    {
                                                        onCameraPermissionUpdated(PermissionStatus.Denied);
                                                    });
#endif
    }

    public void UpdateCameraPermission(string data)
    {
        onCameraPermissionUpdated((PermissionStatus)Enum.Parse(typeof(PermissionStatus), data));
    }

    public static void OpenInAppReview()
    {
#if UNITY_EDITOR
        Debug.Log("OpenInAppReview");
#elif UNITY_IOS
        NativeHelperIOS.OpenInAppReview();
#else
#endif
    }

    public static void PrepareImpactFeedbackGenerator()
    {
#if UNITY_EDITOR
        Debug.Log("PrepareImpactFeedbackGenerator");
#elif UNITY_IOS
        NativeHelperIOS.PrepareImpactFeedbackGenerator();
#else
#endif
    }

    public static void GenerateImpactFeedback()
    {
#if UNITY_EDITOR
        // Debug.Log("GenerateImpactFeedback");
#elif UNITY_IOS
        NativeHelperIOS.GenerateImpactFeedback();
#else
#endif
    }

    // editor on click setting doesn't support static method, so we need one that is not static
    public void GenerateImpactFeedbackForOnClick()
    {
#if UNITY_EDITOR
        Debug.Log("GenerateImpactFeedback");
#elif UNITY_IOS
        NativeHelperIOS.GenerateImpactFeedback();
#else
#endif
    }

    private static T ParseEnum<T>(string input)
    {
        return (T)Enum.Parse(typeof(T), input);
    }
}