using ProGamer;
using UnityEngine;
using UnityEngine.UI;

public class SubmitDisclaimerPopup : SimplePopup
{
    [SerializeField] private Toggle copyrightToggle;
    [SerializeField] private Toggle noInappropriateToggle;
    [SerializeField] private Button continueButton;

    protected override void InitPopup()
    {
        copyrightToggle.isOn = false;
        noInappropriateToggle.isOn = false;
        SetButtonStatus();
        base.InitPopup();
    }

    public void SetButtonStatus()
    {
        continueButton.interactable = copyrightToggle.isOn && noInappropriateToggle.isOn;
    }

    public void OnPressContinue()
    {
        Close();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.SubmitConfirmPopup, true);
    }

    public void OnPressCancel()
    {
        Close();
    }
}