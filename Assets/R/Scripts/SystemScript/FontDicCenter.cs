using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontDicCenter : ProGamer.Singleton<FontDicCenter>
{
    protected LocalizationFontReference mLocalizationFontReference;

    public LocalizationFontReference LocalizationFontDic
    {
        get
        {
            if (mLocalizationFontReference == null)
            {
                mLocalizationFontReference = (Resources.Load("Fonts/LocalizationFontDic") as GameObject).GetComponent<LocalizationFontReference>();
            }

            return mLocalizationFontReference;
        }
    }

    public void ReleaseMemory()
    {
        mLocalizationFontReference = null;

        Resources.UnloadUnusedAssets();
    }
}