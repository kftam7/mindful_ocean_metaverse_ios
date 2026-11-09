using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;

public delegate void ImageSavedCallback(bool isSuccess);

public class ImageToAlbum : ProGamer.Singleton<ImageToAlbum>
{
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern void _ImgToAlbum(string path);
	[DllImport ("__Internal")]
	private static extern void _RegisterImageToAlbumCallback(string receiver, string callback);
	[DllImport ("__Internal")]
	private static extern void _DeregisterImageToAlbumCallback();
#endif

    protected bool isRegisteredCallback = false;

    public event ImageSavedCallback SavedCallback;

    public void StartListenCallback(ImageSavedCallback callback)
    {
        SavedCallback += callback;
    }

    public void StopListenCallback(ImageSavedCallback callback)
    {
        SavedCallback -= callback;
    }

    protected void RegisterCallback()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		_RegisterImageToAlbumCallback(this.gameObject.name, "SavedImageResult");
#endif
    }

    protected void DeregisterCallback()
    {
#if UNITY_IPHONE && !UNITY_EDITOR
		_DeregisterImageToAlbumCallback();
#endif
    }

    public void SaveImage(string path)
    {
        if (!isRegisteredCallback)
        {
            RegisterCallback();
            isRegisteredCallback = true;
        }

#if UNITY_IPHONE && !UNITY_EDITOR
		_ImgToAlbum(path);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
		StartCoroutine(SaveImageToAndroid(path));
#endif

#if UNITY_EDITOR
        SavedImageResult("success");
#endif
    }

    IEnumerator SaveImageToAndroid(string path)
    {
        var www = new WWW(path);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError(string.Format("Can't read file {0}, {1}", path, www.error));
            SavedImageResult("fail");
        }

        try
        {
            string savedPath = ImageDataWriter.SaveImageByte(www.bytes, System.IO.Path.GetFileNameWithoutExtension(path));
            if (!string.IsNullOrEmpty(savedPath))
            {
                ScanFileForAndroid(savedPath);
                SavedImageResult("success");
            }
            else
            {
                SavedImageResult("fail");
            }
        }
        catch (System.Exception ex)
        {
            SavedImageResult("fail: " + ex.Message);
        }
    }

    protected void ScanFileForAndroid(string path)
    {
        using (AndroidJavaClass jcUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject joActivity = jcUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject joContext = joActivity.Call<AndroidJavaObject>("getApplicationContext"))
        using (AndroidJavaClass jcMediaScannerConnection = new AndroidJavaClass("android.media.MediaScannerConnection"))
        {
            Debug.Log("Scan file in: " + path);
            jcMediaScannerConnection.CallStatic("scanFile", joContext, new string[] { path }, null, null);
        }
    }

    public void SavedImageResult(string result)
    {
        Debug.Log("ImageToAlbum: " + result);
        if (SavedCallback != null)
        {
            SavedCallback(result == "success");
        }
    }
}