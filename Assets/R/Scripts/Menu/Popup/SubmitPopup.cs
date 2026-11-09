using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class SubmitPopup : SimplePopup
{
    [SerializeField] private Text appleIdText;

    public void SetAppleId(string appleId)
    {
        appleIdText.text = appleId;
    }

    public void OnPressOk()
    {
        Close();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.SubmitConfirmPopup, true);
    }

    public void OnPressCancel()
    {
        Close();
    }
}