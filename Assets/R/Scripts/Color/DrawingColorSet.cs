using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawingColorSet
{
    public Color[] colors;

    public Color GetColor(int colorIndex)
    {
        return colors[colorIndex];
    }
}