using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ToggleHelper : MonoBehaviour
{
    [SerializeField] private bool isLanguageButton;
    [SerializeField] private string language = "English";
    [SerializeField] protected Sprite onStatus;
    [SerializeField] protected Sprite offStatus;

    [SerializeField] protected UnityEvent onEvent;
    [SerializeField] protected UnityEvent offEvent;

    private void OnEnable()
    {
        if (isLanguageButton)
        {
            string lan = PlayerPrefs.GetString("Language", "English");
            GetComponent<Toggle>().isOn = lan == language;
        }
    }

    public void OnToggleValueChanged(bool isOn)
    {
        GetComponent<Image>().sprite = isOn ? onStatus : offStatus;

        if (isOn)
        {
            onEvent.Invoke();
        }
        else
        {
            offEvent.Invoke();
        }
    }
}