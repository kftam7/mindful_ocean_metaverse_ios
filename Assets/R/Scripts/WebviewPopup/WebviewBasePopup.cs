using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProGamer;

public class WebviewBasePopup : SimplePopup
{
    [SerializeField] protected UIWebView webviewObject;

    public override void PopupDidShow()
    {
        base.PopupDidShow();

        string url = (string)parameters["URL"];
        if (!string.IsNullOrEmpty(url))
        {
            LoadWebview(url);
        }
    }

    protected virtual void OnEnable()
    {
        try
        {
            webviewObject.CallFromJSEvent += WebViewCallExternal;
        }
        catch (System.Exception e)
        {
#if !UNITY_EDITOR
			UnityEngine.Debug.LogException(e);
#endif
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        try
        {
            webviewObject.CallFromJSEvent -= WebViewCallExternal;
        }
        catch (System.Exception e)
        {
#if !UNITY_EDITOR
			UnityEngine.Debug.LogException(e);
#endif
        }
    }

    public void LoadWebview(string url)
    {
        webviewObject.LoadUrlAsync(url);
    }

    public void WebViewCallExternal(string msg)
    {
        Debug.Log("Call external " + msg);
    }
}