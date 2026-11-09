using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleColorHelper : MonoBehaviour
{
    [SerializeField] protected Color onColor;
    [SerializeField] protected Color offColor;

    [SerializeField] protected Text targetObject;

    public void OnToggleValueChanged(bool isOn)
    {
        targetObject.color = isOn ? onColor : offColor;
    }
}