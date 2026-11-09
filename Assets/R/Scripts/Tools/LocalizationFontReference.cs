using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class LocalizationFontReference : MonoBehaviour
{
    [SerializeField] protected Font Default;
    [SerializeField] protected Font English;
    [SerializeField] protected Font French;
    [SerializeField] protected Font German;
    [SerializeField] protected Font Italian;
    [SerializeField] protected Font BrazilianPortuguese;
    [SerializeField] protected Font Portuguese;
    [SerializeField] protected Font Japanese;
    [SerializeField] protected Font LatinSpanish;
    [SerializeField] protected Font Spanish;
    [SerializeField] protected Font Korean;
    [SerializeField] protected Font SimplifiedChinese;
    [SerializeField] protected Font TraditionalChinese;
    [SerializeField] protected Font Turkish;
    [SerializeField] protected Font Russian;
    [SerializeField] protected Font Vietnamese;

    public Font GetFontByLanguage(string lang)
    {
        switch (lang)
        {
            case "English":
                return this.English ? this.English : this.Default;
                break;
            case "French":
                return this.French ? this.French : this.Default;
                break;
            case "German":
                return this.German ? this.German : this.Default;
                break;
            case "Italian":
                return this.Italian ? this.Italian : this.Default;
                break;
            case "BrazilianPortuguese":
                return this.BrazilianPortuguese ? this.BrazilianPortuguese : this.Default;
                break;
            case "Portuguese":
                return this.Portuguese ? this.Portuguese : this.Default;
                break;
            case "Japanese":
                return this.Japanese ? this.Japanese : this.Default;
                break;
            case "LatinSpanish":
                return this.LatinSpanish ? this.LatinSpanish : this.Default;
                break;
            case "Spanish":
                return this.Spanish ? this.Spanish : this.Default;
                break;
            case "Korean":
                return this.Korean ? this.Korean : this.Default;
                break;
            case "SimplifiedChinese":
                return this.SimplifiedChinese ? this.SimplifiedChinese : this.Default;
                break;
            case "TraditionalChinese":
                return this.TraditionalChinese ? this.TraditionalChinese : this.Default;
                break;
            case "Turkish":
                return this.Turkish ? this.Turkish : this.Default;
                break;
            case "Russian":
                return this.Russian ? this.Russian : this.Default;
                break;
            case "Vietnamese":
                return this.Vietnamese ? this.Vietnamese : this.Default;
                break;
            default:
                return this.Default;
                break;
        }
    }
}