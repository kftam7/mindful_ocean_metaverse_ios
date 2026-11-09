using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using ProGamer;

public delegate void PrintingResultCallback(bool success);

public class PrintingManager : ProGamer.Singleton<PrintingManager>
{
    [DllImport("__Internal")]
    private static extern void _RegisterCallback(string str, string callback);

    [DllImport("__Internal")]
    private static extern void _PrintImage(string str);

    protected bool isRegisterCallback = false;

    public const string PrintingMaterialPath = "/PrintingMaterial/";
    public const string DrawingMaterialPath = "/InappColoringPaper/";

    public event PrintingResultCallback OnPrintingProcessDone;

    public bool IsRegisterCallback
    {
        get { return isRegisterCallback; }
    }

    public void RegisterCallback()
    {
#if UNITY_IOS && !UNITY_EDITOR
		Debug.Log("Init: Set callback object " + this.gameObject.name); 
		_RegisterCallback(this.gameObject.name, "PrintingCallback");
#endif

        isRegisterCallback = true;
    }

    public void Print(string path)
    {
        Debug.Log("Print :" + path);
        if (!isRegisterCallback)
        {
            RegisterCallback();
            isRegisterCallback = true;
        }

#if UNITY_IOS && !UNITY_EDITOR
		_PrintImage(path);
#endif

        //Only use for saved image, cannot apply for streaming file
#if UNITY_ANDROID && !UNITY_EDITOR
		PrintInAndroid(path);
#endif
    }

    //Callback for observing when a print operation is completed. When print is finished either the system acquired the document to print or printing was cancelled.
    protected void PrintInAndroid(string path)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject printHelper = new AndroidJavaObject("android.support.v4.print.PrintHelper", currentActivity);

        AndroidJavaClass bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory");
        AndroidJavaObject bitmap = bitmapFactory.CallStatic<AndroidJavaObject>("decodeFile", path);

        printHelper.Call("printBitmap", "Print coloring paper: " + System.IO.Path.GetFileName(path), bitmap);
    }

    public void PrintInAndroid(byte[] data)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject printHelper = new AndroidJavaObject("android.support.v4.print.PrintHelper", currentActivity);

        AndroidJavaClass bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory");
        AndroidJavaObject bitmap = bitmapFactory.CallStatic<AndroidJavaObject>("decodeByteArray", data, 0, data.Length);

        printHelper.Call("printBitmap", "Print coloring paper.", bitmap);
    }

    public void PrintingCallback(string message)
    {
        Debug.Log("Printing Done: " + message);
        if (OnPrintingProcessDone != null)
        {
            bool isSuccess = message == "success";

            Debug.Log("OnPrintingProcessDone: " + isSuccess.ToString());
            OnPrintingProcessDone(isSuccess);
        }
    }

    public static string GetPrintingMaterialPath(string fileName, string folder)
    {
        return Application.streamingAssetsPath + folder + fileName;
    }
}