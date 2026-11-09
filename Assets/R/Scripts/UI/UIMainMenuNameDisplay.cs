using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMainMenuNameDisplay : MonoBehaviour
{
    [SerializeField] private InputField _inputField;

    public void ChangeNameText(string name)
    {
        _inputField.text = name;
    }

    public bool Interactable
    {
        get { return _inputField.interactable; }
        set { _inputField.interactable = value; }
    }

    public void SetInteractable(bool interactable)
    {
        _inputField.interactable = interactable;
    }

    public void AddOnValueChangeHandler(UnityAction<string> val)
    {
        _inputField.onValueChanged.AddListener(val);
    }

    public void SetToFocus()
    {
        EventSystem.current.SetSelectedGameObject(_inputField.gameObject, null);
    }
}