using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class iPhoneXMarginFix : MonoBehaviour
{
    public List<GameObject> iPhoneXMarginObject;

    //Ratio = W / H
    public const float PortraitRatio = 0.462f;
    public const float LandscapeRatio = 2.165f;

    void Start()
    {
        UpdateVisible();
    }

    public void UpdateVisible()
    {
#if UNITY_IOS
        if (iPhoneXMarginObject != null)
        {
            bool deviceIsIphoneX;
#if UNITY_EDITOR
            float ratio = (float)Screen.width / Screen.height;
            if (Screen.width > Screen.height)
            {
                //Landscape
                deviceIsIphoneX = CheckNumber(ratio, LandscapeRatio, 0.1f);
            }
            else
            {
                //Portrait
                deviceIsIphoneX = CheckNumber(ratio, PortraitRatio, 0.1f);
            }
#else
			deviceIsIphoneX = SystemExtension.isIPhoneXScreen();
#endif
            foreach (GameObject go in iPhoneXMarginObject)
            {
                go.SetActive(deviceIsIphoneX);
            }
        }
#endif
    }

    public bool CheckNumber(float number, float target, float error)
    {
        return number >= target * (1f - error) && number <= target * (1f + error);
    }
}