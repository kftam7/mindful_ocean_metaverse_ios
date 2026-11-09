using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(iPhoneXMarginFix))]
public class iPhoneXMarginFixEditor : Editor
{
    void OnEnable()
    {
        (target as iPhoneXMarginFix).UpdateVisible();
    }
}