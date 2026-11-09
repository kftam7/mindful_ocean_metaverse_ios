using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ModelClickEvent : UnityEvent<string> { }

public class AquariumModelButton : MonoBehaviour
{
    public string AquariumId;
    public ModelClickEvent OnClick = new ModelClickEvent();

    void OnMouseDown()
    {
        if (OnClick != null && !string.IsNullOrEmpty(AquariumId))
        {
            OnClick.Invoke(AquariumId);
        }
    }
}