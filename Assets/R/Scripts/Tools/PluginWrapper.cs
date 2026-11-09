using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using Firebase.Storage;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Threading;
using ProGamer;

public delegate void UploadFileCallback(bool isSuccess);

public delegate void DownloadFileCallback(int errorCode, byte[] data);

public class PluginWrapper : ProGamer.Singleton<PluginWrapper>
{
    public const string SkinsStoragePath = "gs://ar-education-game-is-for-k-630.appspot.com/Skins/";
    public const string ThumbnailPath = "gs://ar-education-game-is-for-k-630.appspot.com/Thumbnail/";
    public const string QuestionnairePath = "gs://ar-education-game-is-for-k-630.appspot.com/Questionnaire/";
    public const string ColoringPapersPath = "gs://ar-education-game-is-for-k-630.appspot.com/ColoringPapers/";

    protected bool isFirebaseLoggedin = false;

    void Start()
    {
        Debug.Log("[PluginWrapper] Start");
        FirebaseAuth();
    }

    public void FirebaseAuth()
    {
        // Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        // auth.SignInAnonymouslyAsync().ContinueWith(task => {
        // 	if (task.IsCanceled) {
        // 		Debug.LogError("SignInAnonymouslyAsync was canceled.");
        // 		return;
        // 	}
        // 	if (task.IsFaulted) {
        // 		Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
        // 		return;
        // 	}
        //
        // 	Firebase.Auth.FirebaseUser newUser = task.Result;
        // 	isFirebaseLoggedin = true;
        //
        // 	Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
        // });
    }

    // public void UploadTextFile(string filename, string content, MetadataChange resultData, string path, UploadFileCallback callback = null){
    //
    // 	UploadFile(filename, System.Text.Encoding.UTF8.GetBytes(content), resultData, path, callback);
    // }

    // public void UploadFile(string filename, byte[] data, MetadataChange resultData, string path, UploadFileCallback uploadCallback = null){
    // 	if(isFirebaseLoggedin){
    // 		Debug.Log("Start upload file: " + filename);
    //
    // 		FirebaseStorage storage = FirebaseStorage.DefaultInstance;
    // 		// Create a storage reference from our storage service
    // 		StorageReference storage_ref = storage.GetReferenceFromUrl(path + filename);
    //
    // 		storage_ref.PutBytesAsync(data, resultData)
    // 			.ContinueWith ((Task<StorageMetadata> task) => {
    // 				if (task.IsFaulted || task.IsCanceled) {
    //
    // 					Debug.Log(task.Exception.ToString());
    // 					// Uh-oh, an error occurred!
    // 					if(uploadCallback != null){
    //
    // 						uploadCallback(false);
    // 					}
    //
    // 				} else {
    // 					// Metadata contains file metadata such as size, content-type, and download URL.
    // 					Firebase.Storage.StorageMetadata metadata = task.Result;
    //
    // 					//string download_url = storage_refmetadata.DownloadUrl.ToString();
    // 					//Debug.Log("Finished uploading...");
    // 					//Debug.Log("download url = " + download_url);
    //
    // 					if(uploadCallback != null){
    //
    // 						uploadCallback(true);
    // 					}
    // 				}
    // 			});
    // 	}else{
    //
    // 		Debug.Log("Please sign in firebase.");
    //
    // 		if(uploadCallback != null){
    //
    // 			uploadCallback(false);
    // 		}
    // 	}
    // }

    public void DownloadFile(string filename, string path, DownloadFileCallback downloadCallback = null)
    {
        Debug.Log(string.Format("Start download file: {0}{1}", path, filename));

        if (isFirebaseLoggedin && Application.internetReachability != NetworkReachability.NotReachable)
        {
            // FirebaseStorage storage = FirebaseStorage.DefaultInstance;
            // Create a storage reference from our storage service
            // StorageReference storage_ref = storage.GetReferenceFromUrl(path + filename);

            // Start downloading a file
            const long maxAllowedSize = 1 * 1024 * 1024;
            // storage_ref.GetBytesAsync(maxAllowedSize).ContinueWith((Task<byte[]> task) => {
            // if (task.IsFaulted || task.IsCanceled) {
            //
            // 	// Uh-oh, an error occurred
            // 	StorageException innerEx = task.Exception.InnerExceptions[0] as StorageException;
            // 	if(innerEx != null){
            //
            // 		downloadCallback(innerEx.ErrorCode, null);
            // 	}
            // } else {
            // 	byte[] fileContents = task.Result;
            // 	Debug.Log("Finished downloading!");
            // 	if(downloadCallback != null){
            //
            // 		downloadCallback(0, fileContents);
            // 	}
            // }
            // });
        }
        else
        {
            // downloadCallback(-1, null);
        }
    }


    public string SavedFileName = "";

    public void ImgWriteCallback(string result)
    {
        if (result == "success")
        {
            PopupManager.Instance.ShowMessagePopup("txt_notice", string.Format(Localization.Get("txt_download_success"), SavedFileName));
        }
        else
        {
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_photo_album");
        }
    }
}