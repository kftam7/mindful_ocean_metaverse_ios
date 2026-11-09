/*
 * Copyright (C) 2012 GREE, Inc.
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty.  In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System.Collections;
using UnityEngine;


public class UIWebView : MonoBehaviour
{
    protected WebViewObject webViewObject;

    protected virtual void Awake()
    {
        GameObject go = (new GameObject("WebViewObject"));
        webViewObject = go.AddComponent<WebViewObject>();
        go.transform.SetParent(transform);
        webViewObject.Init(
            cb: (msg) =>
            {
                Debug.Log(string.Format("CallFromJS[{0}]", msg));
                if (CallFromJSEvent != null) CallFromJSEvent(msg);
            },
            err: (msg) =>
            {
                Debug.Log(string.Format("CallOnError[{0}]", msg));
                if (CallOnErrorEvent != null) CallOnErrorEvent(msg);
            },
            ld: (msg) =>
            {
                Debug.Log(string.Format("CallOnLoaded[{0}]", msg));
                if (CallOnLoadedEvent != null) CallOnLoadedEvent(msg);
#if !UNITY_ANDROID
                webViewObject.EvaluateJS(@"
                  window.Unity = {
                    call: function(msg) {
                      var iframe = document.createElement('IFRAME');
                      iframe.setAttribute('src', 'unity:' + msg);
                      document.documentElement.appendChild(iframe);
                      iframe.parentNode.removeChild(iframe);
                      iframe = null;
                    }
                  }
                ");
#endif
            },
            enableWKWebView: true);
    }

    public delegate void UIWebViewEvent(string msg);

    public event UIWebViewEvent CallFromJSEvent;
    public event UIWebViewEvent CallOnErrorEvent;
    public event UIWebViewEvent CallOnLoadedEvent;
    public event UIWebViewEvent CallExternalEvent;

    protected virtual void OnEnable()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(true);
        }
    }

    protected virtual void OnDisable()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
        }
    }

    public void SetMargin(int left, int top, int right, int bottom)
    {
        webViewObject.SetMargins(left, top, right, bottom);
    }

    public void LoadUrlAsync(string url)
    {
        StartCoroutine(LoadUrl(url));
    }

    private IEnumerator LoadUrl(string url)
    {
        Debug.Log("URL: " + url);
        if (url.Contains("external"))
        {
            Debug.Log("It is an external: " + url);
            if (CallExternalEvent != null) CallExternalEvent(url);
        }
        else if (url.StartsWith("http"))
        {
            Debug.Log("Starts with HTTP: " + url);
            webViewObject.LoadURL(url.Replace(" ", "%20"));
        }
        else
        {
            Debug.Log("URL else: " + url);
            var exts = new string[]
            {
                ".jpg",
                ".html" // should be last
            };
            foreach (var ext in exts)
            {
                url = url.Replace(".html", ext);
                var src = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                var dst = System.IO.Path.Combine(Application.persistentDataPath, url);
                byte[] result = null;
                if (src.Contains("://"))
                {
                    // for Android
                    var www = new WWW(src);
                    yield return www;
                    result = www.bytes;
                }
                else
                {
                    result = System.IO.File.ReadAllBytes(src);
                }

                System.IO.File.WriteAllBytes(dst, result);
                if (ext == ".html")
                {
                    webViewObject.LoadURL("file://" + dst.Replace(" ", "%20"));
                    break;
                }
            }
        }
    }
}