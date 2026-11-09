using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SharePopup : ProGamer.SimplePopup
{
    [SerializeField] protected Image screenShot;

    protected override void InitPopup()
    {
        base.InitPopup();
        screenShot.sprite = (Sprite)this.parameters["ScreenShot"];
    }

    public void SaveToDisk() { }

    public void ShareToFB() { }
}