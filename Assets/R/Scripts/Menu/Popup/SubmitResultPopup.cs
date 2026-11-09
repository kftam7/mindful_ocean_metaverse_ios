using ProGamer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SubmitResultPopup : SimplePopup
{
    [SerializeField] private GameObject successNode;
    [SerializeField] private GameObject failedNode;

    public void SetSuccess(bool value)
    {
        successNode.SetActive(value);
        failedNode.SetActive(!value);
    }

    public void OnOpenWebsite()
    {
        Application.OpenURL(GameSetting.Instance.GameWebSite);
    }

    public void OnClose()
    {
        Close();
    }

    public void OnReturnToAquarium()
    {
        if (SceneManager.GetActiveScene().name == "ColoringBook")
        {
            FindObjectOfType<DrawingController>().Back(false);
        }
        else
        {
            Close();
            PopupManager.Instance.ActivePopup.Close();
        }
    }
}