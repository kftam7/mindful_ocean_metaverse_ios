using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARAquariumButton : MonoBehaviour
{
    public Image AquariumPreview;
    public Text AquariumName;

    protected AquariumData data;

    public void SetData(AquariumData d)
    {
        this.data = d;

        Sprite preview = null;

        if (!string.IsNullOrEmpty(d.SnapShotPath))
        {
            preview = ImageDataWriter.LoadSprite(d.SnapShotPath);
        }

        if (preview != null)
        {
            this.AquariumPreview.sprite = preview;

            Vector2 buttonSize = AquariumPreview.rectTransform.sizeDelta;
            this.AquariumPreview.rectTransform.sizeDelta = new Vector2(buttonSize.y / Screen.height * Screen.width, buttonSize.y);
        }
        else
        {
            this.AquariumPreview.sprite = SpriteDicCenter.Instance.ThemeSprite.GetSpriteByName(d.Data.Icon);
        }

        if (d.Index > 0)
        {
            AquariumName.SetLocalization("txt_aquarium", "{0} " + d.Index.ToString("00"));
        }
        else
        {
            AquariumName.SetLocalization(d.AquariumName);
        }
    }
}