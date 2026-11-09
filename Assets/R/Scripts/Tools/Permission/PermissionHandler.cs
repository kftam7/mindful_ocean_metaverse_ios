using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;

public delegate void PermissionCallback();

public class PermissionHandler : ProGamer.Singleton<PermissionHandler>
{
    protected bool isWritingToLibrary = false;

    void Start()
    {
        ImageToAlbum.Instance.StartListenCallback(ImgWriteCallback);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (ImageToAlbum.Instance) ImageToAlbum.Instance.StopListenCallback(ImgWriteCallback);
    }

    public void SaveToCameraRoll(List<string> selectedImage)
    {
        if (!GameSave.HasShownPhotoAlbumPermission)
        {
            GameSave.HasShownPhotoAlbumPermission = true;

#if UNITY_ANDROID
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_photo_album", () => {

                if (!AndroidPermissionsManager.IsPermissionGranted(GameSetting.WriteStoragePermission))
                {

                    AndroidPermissionsManager.RequestPermission(new string[] { GameSetting.WriteStoragePermission }, (permission) => {

                        SaveToCameraRoll(selectedImage);
                    },(denied)=>{
                        //trigger fail to next tutorial
                        EventManager.TriggerEvent(EventList.OnPrintDone, false);
                    });
                }
                else
                {
                    SaveToCameraRoll(selectedImage);
                }
            });
#else
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_photo_album", () =>
            {
                SaveToCameraRoll(selectedImage);
            });
#endif
        }
        else
        {
#if UNITY_ANDROID
            if (!AndroidPermissionsManager.IsPermissionGranted(GameSetting.WriteStoragePermission))
            {

                PopupManager.Instance.ShowPermissionPopup("txt_permissions_title", Localization.Get("txt_permission_write_storage_for_sketches"), () => {

                    AndroidPermissionsManager.RequestPermission(new string[] { GameSetting.WriteStoragePermission }, (permission) => {
                        
                        SaveToCameraRoll(selectedImage);
                    });
                });

                return;
            }
#endif
            if (!isWritingToLibrary)
            {
                isWritingToLibrary = true;
                // Debug.Log("selectedImage  "+selectedImage)
                foreach (string s in selectedImage)
                {
                    Debug.Log("selectedImage  " + PrintingManager.GetPrintingMaterialPath(s, PrintingManager.PrintingMaterialPath));

                    ImageDataWriter.SaveImgToGallery(PrintingManager.GetPrintingMaterialPath(s, PrintingManager.PrintingMaterialPath));
                }
            }
        }
    }

    public void SaveFillColorToCameraRoll(List<string> selectedImage)
    {
        if (!GameSave.HasShownPhotoAlbumPermission)
        {
            GameSave.HasShownPhotoAlbumPermission = true;

#if UNITY_ANDROID
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_photo_album", () => {

                if (!AndroidPermissionsManager.IsPermissionGranted(GameSetting.WriteStoragePermission))
                {

                    AndroidPermissionsManager.RequestPermission(new string[] { GameSetting.WriteStoragePermission }, (permission) => {

                        SaveFillColorToCameraRoll(selectedImage);
                    },(denied)=>{
                        //trigger fail to next tutorial
                        EventManager.TriggerEvent(EventList.OnPrintDone, false);
                    });
                }
                else
                {
                    SaveFillColorToCameraRoll(selectedImage);
                }
            });
#else
            PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_photo_album", () =>
            {
                SaveFillColorToCameraRoll(selectedImage);
            });
#endif
        }
        else
        {
#if UNITY_ANDROID
            if (!AndroidPermissionsManager.IsPermissionGranted(GameSetting.WriteStoragePermission))
            {

                PopupManager.Instance.ShowPermissionPopup("txt_permissions_title", Localization.Get("txt_permission_write_storage_for_sketches"), () => {

                    AndroidPermissionsManager.RequestPermission(new string[] { GameSetting.WriteStoragePermission }, (permission) => {

                        SaveFillColorToCameraRoll(selectedImage);
                    });
                });

                return;
            }
#endif
            if (!isWritingToLibrary)
            {
                isWritingToLibrary = true;
                // Debug.Log("selectedImage  "+selectedImage)
                foreach (string s in selectedImage)
                {
                    Debug.Log("selectedImage  " + ImageDataWriter.GetSaveFilePath(s));

                    ImageDataWriter.SaveImgToGallery(ImageDataWriter.GetSaveFilePath(s)); //PrintingManager.GetPrintingMaterialPath(s, PrintingManager.PrintingMaterialPath));
                }
            }
        }
    }

    protected bool blockCallback = false;

    public void ImgWriteCallback(bool success)
    {
        if (!blockCallback)
        {
            blockCallback = true;

            if (success)
            {
                PopupManager.Instance.ShowMessagePopup("txt_paper_downloaded_title", "txt_paper_downloaded_desc", () =>
                {
                    EventManager.TriggerEvent(EventList.OnPrintDone, true);
                });
            }
            else
            {
                PopupManager.Instance.ShowMessagePopup("txt_permissions_title", "txt_permission_photo_album", () =>
                {
                    EventManager.TriggerEvent(EventList.OnPrintDone, false);
                });
            }

            StartCoroutine(WriteCallback());
        }
    }

    IEnumerator WriteCallback()
    {
        //dont allow print too frequency
        yield return new WaitForSeconds(1f);

        blockCallback = false;
        isWritingToLibrary = false;
    }
}