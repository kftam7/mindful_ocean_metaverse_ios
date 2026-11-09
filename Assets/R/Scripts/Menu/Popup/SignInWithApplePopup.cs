using System;
using ProGamer;

public class SignInWithApplePopup : SimplePopup
{
    public Action OnSuccess;
    private SimplePopup processingPopup;

    public void OnPressQuestion()
    {
        Close();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.SignInWithAppleWhyPopup, true);
    }

    public void OnPressOk()
    {
        Close();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.ProcessingPopup, true);
        processingPopup = PopupManager.Instance.ActivePopup;
        ServerCenter.Instance.SignInWithApple(
            _ => // success
            {
                if (!GameSave.TermAccepted)
                {
                    processingPopup.Close();
                    PopupManager.Instance.QuickShowPopup(PopupPrefab.DisclaimerPopup, true);
                    var popup = PopupManager.Instance.ActivePopup as DisclaimerPopup;
                    popup.OnSuccess = OnSuccess;
                }
                else
                {
                    processingPopup.Close();
                    OnSuccess?.Invoke();
                }
            },
            () => // failed
            {
                processingPopup.Close();
                PopupManager.Instance.ShowMessagePopup("txt_notice", "txt_login_fail", null, true);
            },
            () => // banned
            {
                processingPopup.Close();
                PopupManager.Instance.ShowBadMessagePopup("txt_login_banned", null, true);
            });
    }

    public void OnPressCancel()
    {
        Close();
    }
}