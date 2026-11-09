using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageSelection : MonoBehaviour
{
    [SerializeField] protected PageSwitcher mPageSwitcher;


    public void SetLanguage(string lan)
    {
        ProGamer.Localization.language = lan;
        mPageSwitcher.NextPage();
    }
}