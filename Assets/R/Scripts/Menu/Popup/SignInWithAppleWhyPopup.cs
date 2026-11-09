using ProGamer;

public class SignInWithAppleWhyPopup : SimplePopup
{
    public void OnPressOk()
    {
        Close();
        PopupManager.Instance.QuickShowPopup(PopupPrefab.SignInWithApplePopup, true);
    }
}