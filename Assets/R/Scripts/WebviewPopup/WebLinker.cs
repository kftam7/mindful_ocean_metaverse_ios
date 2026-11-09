using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

[RequireComponent(typeof(Button))]
public class WebLinker : MonoBehaviour
{
    [SerializeField] protected string link;
    [SerializeField] protected bool ingameWebView = true;
    [SerializeField] protected bool isFullPage = false;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OpenLink);
    }

    public void OpenLink()
    {
        if (ingameWebView)
        {
            if (isFullPage)
            {
                PopupManager.Instance.QuickShowFullPopupWebview(link);
            }
            else
            {
                PopupManager.Instance.QuickShowPopupWebview(link);
            }
        }
        else
        {
            Application.OpenURL(link);
        }
    }
}