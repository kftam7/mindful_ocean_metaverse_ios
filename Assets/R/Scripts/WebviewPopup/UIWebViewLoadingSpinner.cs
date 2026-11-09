using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIWebViewLoadingSpinner : UILoadingSpinner
{
    [SerializeField] private UIWebView webView;
    [SerializeField] private Image image;


    void OnEnable()
    {
        webView.CallOnLoadedEvent += CallOnLoaded;
        image.enabled = true;
    }

    void OnDisable()
    {
        webView.CallOnLoadedEvent -= CallOnLoaded;
    }

    public void CallOnLoaded(string msg)
    {
        image.enabled = false;
    }
}