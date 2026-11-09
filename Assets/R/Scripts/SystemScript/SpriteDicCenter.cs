using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDicCenter : ProGamer.Singleton<SpriteDicCenter>
{
    protected SpriteReference mThemeSprite;

    public SpriteReference ThemeSprite
    {
        get
        {
            if (mThemeSprite == null)
            {
                mThemeSprite = (Resources.Load("SpriteDic/ThemeSprite") as GameObject).GetComponent<SpriteReference>();
            }

            return mThemeSprite;
        }
    }

    protected SpriteReference mPrintingMaterialSpriteDic;

    public SpriteReference PrintingMaterialSpriteDic
    {
        get
        {
            if (mPrintingMaterialSpriteDic == null)
            {
                mPrintingMaterialSpriteDic = (Resources.Load("SpriteDic/PrintingMaterialSpriteDic") as GameObject).GetComponent<SpriteReference>();
            }

            return mPrintingMaterialSpriteDic;
        }
    }

    public void ReleaseMemory()
    {
        mThemeSprite = null;
        mPrintingMaterialSpriteDic = null;

        Resources.UnloadUnusedAssets();
    }
}