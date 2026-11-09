using UnityEngine;
using System.Collections;
using ProGamer;

public class PopupUIWebView : UIWebView
{
    [SerializeField] private SimplePopup parentPopup;

    protected override void OnEnable()
    {
        //Base on popup animation, will not call base.OnEnable

        parentPopup.PopupDidShowEvent += PopupDidShow;
        parentPopup.PopupWillCloseEvent += PopupWillClose;
        CallOnLoadedEvent += CallOnLoaded;
    }

    protected override void OnDisable()
    {
        //Base on popup animation, will not call base.OnDisable

        parentPopup.PopupDidShowEvent -= PopupDidShow;
        parentPopup.PopupWillCloseEvent -= PopupWillClose;
        CallOnLoadedEvent -= CallOnLoaded;
    }

    public void PopupDidShow()
    {
        Vector3[] worldCorners = new Vector3[4];
        (transform as RectTransform).GetWorldCorners(worldCorners);
        Vector2[] screenPos = new Vector2[4];

        for (int i = 0; i < worldCorners.Length; i++)
        {
            screenPos[i] = RectTransformUtility.WorldToScreenPoint(Camera.main, worldCorners[i]);
        }

        SetMargin((int)screenPos[0].x, (int)(Camera.main.pixelRect.height - screenPos[1].y), (int)(Camera.main.pixelRect.width - screenPos[2].x), (int)screenPos[0].y);
    }

    public void PopupWillClose()
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
            this.webViewObject.SetVisibility(true);
        }
    }
}