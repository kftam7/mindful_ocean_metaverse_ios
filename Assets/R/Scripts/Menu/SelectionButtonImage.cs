using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class SelectionButtonImage : MonoBehaviour
{
    [SerializeField] protected bool autoGetStatusSprite = false;
    [SerializeField] protected Sprite normalImage;
    [SerializeField] protected Sprite selectedImage;
    [SerializeField] protected int order = 100;

    public int Order
    {
        get { return order; }
    }

    protected SelectionButtonGroup buttonGroup;
    protected Button _targetButton;

    public Button targetButton
    {
        get
        {
            if (_targetButton == null)
            {
                _targetButton = GetComponent<Button>();
            }

            return _targetButton;
        }
    }

    protected int buttonId = -1;

    public void Init(SelectionButtonGroup group, int id)
    {
        buttonGroup = group;
        buttonId = id;

        if (autoGetStatusSprite)
        {
            normalImage = targetButton.image.sprite;
            selectedImage = targetButton.spriteState.pressedSprite;
        }
    }

    public void OnClick()
    {
        if (buttonId != -1)
        {
            buttonGroup.ButtonClick(buttonId);
        }
    }

    public void OnButtonClick(int id)
    {
        if (buttonId != -1)
        {
            targetButton.image.sprite = (id == buttonId ? selectedImage : normalImage);
        }
    }

    public void AddListener()
    {
        targetButton.onClick.AddListener(OnClick);
    }
}