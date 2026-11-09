using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeHelperIOS
{
    [DllImport("__Internal")]
    public static extern void OpenSetting();

    [DllImport("__Internal")]
    public static extern string GetCameraPermission();

    [DllImport("__Internal")]
    public static extern void RequestCameraPermission();

    [DllImport("__Internal")]
    public static extern void OpenInAppReview();

    [DllImport("__Internal")]
    public static extern void PrepareImpactFeedbackGenerator();

    [DllImport("__Internal")]
    public static extern void GenerateImpactFeedback();
}