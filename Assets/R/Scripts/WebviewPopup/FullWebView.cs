using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ProGamer;

public class FullWebView : UIWebView
{
    [SerializeField] protected GUISkin skin;
    [SerializeField] protected Texture close;
    [SerializeField] protected UnityEvent onCloseClicked;

    void OnGUI()
    {
        GUI.skin = skin;
        GUI.Box(new Rect(0, 0, Screen.width, close.height), "");

        if (GUI.Button(new Rect(Screen.width - close.width, 0, close.width, close.height), close))
        {
            if (onCloseClicked != null)
            {
                Close();
                onCloseClicked.Invoke();
            }
        }
    }

    protected override void OnEnable()
    {
        //Base on popup animation, will not call base.OnEnable
        CallOnLoadedEvent += CallOnLoaded;
    }

    protected override void OnDisable()
    {
        //Base on popup animation, will not call base.OnDisable
        CallOnLoadedEvent -= CallOnLoaded;
    }

    public void Close()
    {
        if (this.webViewObject != null)
        {
            this.webViewObject.SetVisibility(false);
        }
    }

    protected virtual void CallOnLoaded(string msg)
    {
        if (this.webViewObject != null)
        {
            SetMargin(0, close.height, 0, 0);
            this.webViewObject.SetVisibility(true);
        }
    }
}