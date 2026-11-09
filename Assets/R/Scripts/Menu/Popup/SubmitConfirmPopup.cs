using System;
using Firebase.Analytics;
using ProGamer;

public class SubmitConfirmPopup : SimplePopup
{
    private SimplePopup processingPopup;

    public void OnPressConfirm()
    {
        FirebaseAnalytics.LogEvent(AnalyticsEnums.EventConfirmSubmit);
        PopupManager.Instance.QuickShowPopup(PopupPrefab.ProcessingPopup, true);
        processingPopup = PopupManager.Instance.ActivePopup;
        ServerCenter.Instance.SubmitArtwork(DataCenter.Instance.CurrentArtwork,
            (imageData, success) =>
            {
                if (success)
                {
                    imageData.Submitted = true;
                    if (imageData.InAquarium)
                    {
                        var fishTank = FindObjectOfType<FishTank>();
                        if (fishTank != null)
                        {
                            FishEntity fishEntity = fishTank.FindFish(imageData);
                            if (fishEntity != null)
                            {
                                fishTank.RemoveFish(fishEntity);
                            }
                        }
                    }

                    imageData.InAquarium = false;
                    ImageDataManager.Instance.FillImageDataLists(true);
                    ImageDataManager.Instance.SaveDataToDisk();
                    GameSave.SubmitArtworkTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    FirebaseAnalytics.LogEvent(AnalyticsEnums.EventSubmitSuccess);
                }

                processingPopup.Close();
                Close();
                PopupManager.Instance.QuickShowPopup(PopupPrefab.SubmitResultPopup, true);
                ((SubmitResultPopup)PopupManager.Instance.ActivePopup).SetSuccess(success);
            }, () =>
            {
                processingPopup.Close();
                PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_submit_fail", null, true);
            });
    }

    public void OnReturnToAquarium()
    {
        Close();
        PopupManager.Instance.ActivePopup.Close();
    }
}