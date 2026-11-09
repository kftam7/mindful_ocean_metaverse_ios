using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using ProGamer;
using System.Text.RegularExpressions;

public static class SystemExtension
{
    public static string AddCaption(this string content, string caption)
    {
        return caption + "_" + content;
    }

    public static bool IsNaN(this Vector3 vector3)
    {
        return float.IsNaN(vector3.x) || float.IsNaN(vector3.y) || float.IsNaN(vector3.z);
    }

    public static bool IsNaN(this Vector2 vector2)
    {
        return float.IsNaN(vector2.x) || float.IsNaN(vector2.y);
    }

    public static Vector2 Clamp01(this Vector2 uv)
    {
        return new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));
    }

    public static Vector3 ToVector3(this float[] array)
    {
        if (array.Length > 3 || array.Length <= 1)
        {
            throw new System.InvalidCastException();
        }

        if (array.Length == 2)
        {
            return new Vector3(array[0], array[1]);
        }
        else
        {
            return new Vector3(array[0], array[1], array[2]);
        }
    }

    public static Vector3 ToVector3(this List<float> list)
    {
        return list.ToArray().ToVector3();
    }

    public static T ReadSave<T>(this string saveData) where T : new()
    {
        if (!string.IsNullOrEmpty(saveData))
        {
            try
            {
                T dic = JsonConvert.DeserializeObject<T>(saveData);
                return dic;
            }
            catch (System.Exception ex)
            {
                Debug.Log("Cannot load file: " + ex.Message);

                return new T();
            }
        }
        else
        {
            return new T();
        }
    }

    public static float GetWidth(this RectTransform rect)
    {
        Vector3[] vector3s = new Vector3[4];
        rect.GetLocalCorners(vector3s);

        return vector3s[3].x - vector3s[0].x;
    }

    public static float GetHeight(this RectTransform rect)
    {
        Vector3[] vector3s = new Vector3[4];
        rect.GetLocalCorners(vector3s);

        return vector3s[1].y - vector3s[0].y;
    }

    public static Vector2 GetSize(this RectTransform rect)
    {
        Vector3[] vector3s = new Vector3[4];
        rect.GetLocalCorners(vector3s);

        return new Vector2(vector3s[3].x - vector3s[0].x, vector3s[1].y - vector3s[0].y);
    }

    public static Transform FindRecursive(this Transform target, string name)
    {
        Transform result = target.Find(name);

        if (result != null)
        {
            return result;
        }
        else
        {
            foreach (Transform t in target)
            {
                result = FindRecursive(t, name);
                if (result != null)
                {
                    return result;
                }
            }
        }

        return result;
    }


    public static Vector3 ClearY(this Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    public static Vector3 ClearZ(this Vector3 vector)
    {
        return new Vector3(vector.x, vector.y, 0);
    }

    public static bool isIPhoneXScreen()
    {
        if (SystemInfo.deviceModel.IndexOf("iPhone") > -1)
        {
            double screenRatio;
            if (Screen.height < Screen.width)
            {
                screenRatio = (1.0 * Screen.width) / (1.0 * Screen.height);
            }
            else
            {
                screenRatio = (1.0 * Screen.height) / (1.0 * Screen.width);
            }

            return (screenRatio > 2.1 && screenRatio < 2.2);
        }

        return false;
    }

    public static bool isFastCPU()
    {
        Debug.Log("device model : " + SystemInfo.deviceModel);
        // ToDO: we assumed we are using ARKit Devices, which at least have A9 or better chips.
        // for ios devices with A9 / A9X chips, we consider them slow chips than other like A10 etc
        // So we have not yet handled Android models here
        if (SystemInfo.deviceModel.IndexOf("iPad6") > -1 ||
            SystemInfo.deviceModel.IndexOf("iPhone8") > -1)
        {
            return false;
        }

        return true;
    }
}