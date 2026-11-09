using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SelectionButtonGroup : MonoBehaviour
{
    [SerializeField] protected SelectionButtonImage[] buttonGroup;

    void Awake()
    {
        for (int i = 0; i < buttonGroup.Length; i++)
        {
            buttonGroup[i].Init(this, i);
        }
    }

    public void ButtonClick(int id)
    {
        foreach (SelectionButtonImage b in buttonGroup)
        {
            b.OnButtonClick(id);
        }
    }

    public void ResetListener()
    {
        ButtonClick(-1);

        foreach (SelectionButtonImage b in buttonGroup)
        {
            b.AddListener();
        }
    }
}