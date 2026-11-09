using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ProGamer;

[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class UIFontSetter : FontSetter
{
    [SerializeField] protected Text textField;

    public Text TextField
    {
        get
        {
            if (textField == null) textField = GetComponent<Text>();
            return textField;
        }
    }

    public override void OnLocalize()
    {
        SetFontSize();
        Localization.SetFont(TextField);
    }

    public override void SetFontSize()
    {
        if (baseFontSize == -1)
        {
            isBestFit = TextField.resizeTextForBestFit;

            if (isBestFit) baseFontSize = TextField.resizeTextMaxSize;
            else baseFontSize = TextField.fontSize;
        }

        if (isBestFit) TextField.resizeTextMaxSize = (int)baseFontSize + Localization.instance.CurrentFontSizeOffset;
        else TextField.fontSize = (int)baseFontSize + Localization.instance.CurrentFontSizeOffset;
    }
}