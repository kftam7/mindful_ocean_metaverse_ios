using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class FishHintPopup : SimplePopup
{
    [SerializeField] private Text title;
    [SerializeField] private Text description;
    [SerializeField] private GameObject purchaseButton;

    public void ShowTitle(string key)
    {
        title.gameObject.SetActive(true);
        description.gameObject.SetActive(false);
        Localization.SetText(key, title);
    }

    public void ShowText(string key)
    {
        title.gameObject.SetActive(false);
        description.gameObject.SetActive(true);
        Localization.SetText(key, description);
    }

    public override void Close()
    {
        NativeHelper.GenerateImpactFeedback();
        base.Close();
    }

    public void OpenPurchasePopup()
    {
        Close();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.PurchasePopup, true);
        var popup = PopupManager.Instance.ActivePopup as PurchasePopup;
        popup.ShowBandItems();
        popup.PurchaseSuccessCallback = () =>
        {
            ImageDataManager.Instance.FillImageDataLists(true);
        };
    }

    public void SetPurchaseButton(bool value)
    {
        purchaseButton.SetActive(value);
    }
}