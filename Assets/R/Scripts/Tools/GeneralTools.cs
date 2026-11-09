using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IndicatorDirection
{
    Top = 0,
    Right = 3,
    Bottom = 2,
    Left = 1
}

public static class GeneralTools
{
    public static Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public static GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent)
    {
        return GameObject.Instantiate<GameObject>(Resources.Load(path) as GameObject, position, rotation, parent);
    }

    public static GameObject Instantiate(string path, Transform parent)
    {
        return GameObject.Instantiate<GameObject>(Resources.Load(path) as GameObject, parent);
    }

    public static Vector3 GetTop(RectTransform t)
    {
        Vector3[] corners = new Vector3[4];
        t.GetWorldCorners(corners);

        return (corners[1] + corners[2]) / 2f;
    }

    public static Vector3 GetPositon(IndicatorDirection direction, RectTransform t)
    {
        Vector3[] corners = new Vector3[4];
        t.GetWorldCorners(corners);

        switch (direction)
        {
            case IndicatorDirection.Top:

                return (corners[1] + corners[2]) / 2f;
            case IndicatorDirection.Right:

                return (corners[2] + corners[3]) / 2f;
            case IndicatorDirection.Bottom:

                return (corners[3] + corners[0]) / 2f;
            case IndicatorDirection.Left:

                return (corners[0] + corners[1]) / 2f;
        }

        return Vector3.zero;
    }

    public static T GetTargetByName<T>(GameObject go, string name) where T : Object
    {
        T[] list = go.GetComponentsInChildren<T>();
        foreach (T t in list)
        {
            if (t.name == name)
            {
                return t;
            }
        }

        return null;
    }

    public static GameObject Find(this Transform parent, string target)
    {
        return parent.Find(target).gameObject;
    }

    public static bool EqualCompare(this float origin, float target, float error = 0.01f)
    {
        return origin > target * (1 - error) && origin < target * (1 + error);
    }
}