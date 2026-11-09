using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StyleSelectButton : MonoBehaviour
{
    [SerializeField] protected Image styleIcon;
    [SerializeField] protected Text styleName;
    [SerializeField] protected GameObject ConfirmButton;

    protected CollectionData data;

    public void SetButton(CollectionData data)
    {
        this.data = data;

        styleName.SetLocalization(data.ThemeName);
        styleIcon.sprite = SpriteDicCenter.Instance.ThemeSprite.GetSpriteByName(data.StyleIcon);
        SetButtonActive(false);
    }

    public void SetButtonActive(bool isActive)
    {
        ConfirmButton.SetActive(isActive);
    }

    public void OnClick()
    {
        StyleSelection.Instance.OpenNewBook(data);
    }
}