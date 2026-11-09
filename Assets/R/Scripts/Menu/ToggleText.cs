using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProGamer;

[RequireComponent(typeof(Toggle))]
public class ToggleText : MonoBehaviour
{
    [SerializeField] string onTextKey;
    [SerializeField] string offTextKey;
    [SerializeField] Localize localizeText;

    private void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        if (toggle)
        {
            toggle.onValueChanged.AddListener(OnValueChanged);
            OnValueChanged(toggle.isOn);
        }
    }

    protected void OnValueChanged(bool isOn)
    {
        localizeText.Set(isOn ? onTextKey : offTextKey);
    }
}