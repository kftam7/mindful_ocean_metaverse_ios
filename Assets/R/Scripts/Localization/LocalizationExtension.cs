using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using ProGamer;

public static class LocalizationExtension
{
    public static void SetLocalization(this Text text, string key)
    {
        Localize localize = text.AddLocalize();
        localize.Set(key);
    }

    public static void SetLocalization(this Text text, string key, object[] parameters)
    {
        Localize localize = text.AddLocalize();
        localize.Set(key, parameters);
    }

    public static void SetLocalization(this Text text, string key, string format)
    {
        Localize localize = text.AddLocalize();
        localize.Set(key, format);
    }

    public static Localize AddLocalize(this Text text)
    {
        Localize localize = text.GetComponent<Localize>();
        if (!localize)
        {
            localize = text.gameObject.AddComponent<UILocalize>();
        }

        return localize;
    }
}