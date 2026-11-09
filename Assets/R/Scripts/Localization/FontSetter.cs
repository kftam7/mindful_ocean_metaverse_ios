using UnityEngine;
using System.Collections;
using ProGamer;

public abstract class FontSetter : MonoBehaviour
{
    protected float baseFontSize = -1;
    protected bool isBestFit;

    protected virtual void OnEnable()
    {
        OnLocalize();
        Localization.instance.LocalizeUpdate += OnLocalize;
    }

    protected virtual void OnDisable()
    {
        Localization.instance.LocalizeUpdate -= OnLocalize;
    }

    public abstract void OnLocalize();

    public abstract void SetFontSize();
}