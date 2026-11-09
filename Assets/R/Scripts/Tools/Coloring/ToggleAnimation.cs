using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer.UIAnimation;

[RequireComponent(typeof(Toggle))]
public class ToggleAnimation : UITweenPopupAnimation
{
    public Toggle TargetToggle
    {
        get { return target; }
    }

    Toggle target;

    void Awake()
    {
        target = GetComponent<Toggle>();

        if (target != null)
        {
            target.onValueChanged.AddListener(OnValueChanged);
        }
    }

    void OnValueChanged(bool isOn)
    {
        if (isOn)
        {
            OnAnimation();
        }
        else
        {
            OffAnimation();
        }
    }

    public void OnAnimation()
    {
        FadeIn();
    }

    public void OffAnimation()
    {
        FadeOut();
    }
}